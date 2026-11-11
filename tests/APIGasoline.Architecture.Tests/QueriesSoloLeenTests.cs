using System.Reflection;
using System.Reflection.Emit;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Carros.Commands;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Architecture.Tests;

/// <summary>
/// Una Query solo lee. Se revisa el IL compilado de cada QueryHandler (incluido el código async
/// y las lambdas) para que no guarde cambios, no abra transacciones, no publique eventos,
/// no suba ni borre archivos y no cree Commands.
/// </summary>
public sealed class QueriesSoloLeenTests
{
    private static readonly string[] EscriturasDelUnitOfWork =
    [
        nameof(IUnitOfWork.SaveChangesAsync),
        nameof(IUnitOfWork.BeginTransactionAsync),
        nameof(IUnitOfWork.CommitTransactionAsync),
        nameof(IUnitOfWork.RollbackTransactionAsync),
    ];

    private static readonly string[] EscriturasDelAlmacenamiento =
    [
        nameof(IFileStorageService.SubirAsync),
        nameof(IFileStorageService.EliminarAsync),
        nameof(IFileStorageService.IntentarEliminarAsync),
    ];

    public static TheoryData<string> HandlersDeQueries => new(
        Ensamblados.Handlers
            .Where(h => Ensamblados.EsQuery(Ensamblados.RequestDe(h)))
            .Select(Ensamblados.Nombre));

    [Theory]
    [MemberData(nameof(HandlersDeQueries))]
    public void El_handler_de_una_query_no_escribe(string handler)
    {
        var tipo = Ensamblados.Application.GetType(handler, throwOnError: true)!;

        var escrituras = Escrituras(tipo).ToList();

        Assert.True(escrituras.Count == 0,
            $"{tipo.Name} es de una Query pero escribe: {string.Join(", ", escrituras)}. " +
            "Si necesita cambiar algo, conviértelo en Command.");
    }

    /// <summary>Control: si el lector de IL dejara de ver las escrituras, la prueba de arriba pasaría sin revisar nada.</summary>
    [Fact]
    public void Control_el_lector_de_IL_detecta_las_escrituras_de_un_command()
    {
        var escrituras = Escrituras(typeof(DeleteCarroCommandHandler)).ToList();

        Assert.Contains("IUnitOfWork.SaveChangesAsync", escrituras);
    }

    internal static IEnumerable<string> Escrituras(Type tipo)
        => LectorIL.MiembrosUsados(tipo)
            .Select(uso => DescribirEscritura(uso.Codigo, uso.Miembro))
            .OfType<string>()
            .Distinct();

    private static string? DescribirEscritura(OpCode codigo, MemberInfo miembro)
    {
        if (miembro is not MethodBase metodo || metodo.DeclaringType is not { } tipo)
            return null;

        if (tipo == typeof(IUnitOfWork) && EscriturasDelUnitOfWork.Contains(metodo.Name))
            return $"IUnitOfWork.{metodo.Name}";

        if (tipo == typeof(IFileStorageService) && EscriturasDelAlmacenamiento.Contains(metodo.Name))
            return $"IFileStorageService.{metodo.Name}";

        if ((tipo == typeof(IPublisher) || tipo == typeof(IMediator)) && metodo.Name == nameof(IPublisher.Publish))
            return $"{tipo.Name}.Publish";

        if (codigo == OpCodes.Newobj && Ensamblados.EsCommand(tipo))
            return $"new {tipo.Name}";

        return null;
    }
}
