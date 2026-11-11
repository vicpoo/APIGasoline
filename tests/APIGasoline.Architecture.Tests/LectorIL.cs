using System.Reflection;
using System.Reflection.Emit;

namespace APIGasoline.Architecture.Tests;

/// <summary>
/// Lee el IL compilado de una clase y devuelve los métodos, campos y tipos que usa.
/// Incluye los tipos que genera el compilador dentro de ella (la máquina de estados de los
/// métodos async y las clases de las lambdas), así "await _unitOfWork.SaveChangesAsync()" se ve.
/// </summary>
internal static class LectorIL
{
    private static readonly Dictionary<short, OpCode> CodigosDeOperacion = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(f => (OpCode)f.GetValue(null)!)
        .ToDictionary(o => o.Value);

    private const BindingFlags Todo =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    /// <summary>Instrucciones que usa la clase: (código de operación, miembro al que apunta).</summary>
    public static IEnumerable<(OpCode Codigo, MemberInfo Miembro)> MiembrosUsados(Type tipo)
    {
        foreach (var t in ConTiposAnidados(tipo))
        {
            var metodos = t.GetMethods(Todo).Cast<MethodBase>().Concat(t.GetConstructors(Todo));
            foreach (var metodo in metodos)
            foreach (var uso in MiembrosUsados(metodo))
                yield return uso;
        }
    }

    private static IEnumerable<Type> ConTiposAnidados(Type tipo)
    {
        yield return tipo;
        foreach (var anidado in tipo.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
        foreach (var t in ConTiposAnidados(anidado))
            yield return t;
    }

    private static IEnumerable<(OpCode, MemberInfo)> MiembrosUsados(MethodBase metodo)
    {
        var il = metodo.GetMethodBody()?.GetILAsByteArray();
        if (il is null)
            yield break;

        var argsTipo = metodo.DeclaringType is { IsGenericType: true } d ? d.GetGenericArguments() : null;
        var argsMetodo = metodo.IsGenericMethod ? metodo.GetGenericArguments() : null;

        var i = 0;
        while (i < il.Length)
        {
            short valor = il[i++];
            if (valor == 0xFE)
                valor = unchecked((short)(0xFE00 | il[i++]));

            if (!CodigosDeOperacion.TryGetValue(valor, out var codigo))
                throw new InvalidOperationException($"Código IL desconocido 0x{valor:X} en {metodo.DeclaringType}.{metodo.Name}");

            switch (codigo.OperandType)
            {
                case OperandType.InlineMethod:
                case OperandType.InlineField:
                case OperandType.InlineType:
                case OperandType.InlineTok:
                    var token = BitConverter.ToInt32(il, i);
                    i += 4;
                    MemberInfo? miembro;
                    try
                    {
                        miembro = metodo.Module.ResolveMember(token, argsTipo, argsMetodo);
                    }
                    catch (ArgumentException)
                    {
                        miembro = null;
                    }
                    if (miembro is not null)
                        yield return (codigo, miembro);
                    break;
                case OperandType.InlineSwitch:
                    var casos = BitConverter.ToInt32(il, i);
                    i += 4 + 4 * casos;
                    break;
                default:
                    i += TamañoDelOperando(codigo.OperandType);
                    break;
            }
        }
    }

    private static int TamañoDelOperando(OperandType tipo) => tipo switch
    {
        OperandType.InlineNone => 0,
        OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
        OperandType.InlineVar => 2,
        OperandType.InlineI8 or OperandType.InlineR => 8,
        _ => 4, // InlineBrTarget, InlineI, InlineSig, InlineString, ShortInlineR
    };
}
