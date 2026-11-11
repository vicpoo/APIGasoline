using System.Reflection;
using System.Runtime.CompilerServices;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Architecture.Tests;

/// <summary>
/// Reglas del patrón Commands / Queries (CQRS) con MediatR:
/// - Todo request es un <see cref="ICommand"/> o un <see cref="IQuery{TResponse}"/> (nunca IRequest a secas).
/// - Es un <c>sealed record</c> inmutable, con el nombre y la carpeta correctos.
/// - Lo atiende exactamente un handler <c>sealed</c> que se llama igual + "Handler".
/// </summary>
public sealed class CommandsYQueriesTests
{
    [Fact]
    public void Se_encontraron_los_commands_y_queries()
    {
        // Si la reflexión dejara de encontrarlos, las demás reglas pasarían sin revisar nada
        Assert.True(Ensamblados.Commands.Count >= 50, $"Solo se encontraron {Ensamblados.Commands.Count} commands.");
        Assert.True(Ensamblados.Queries.Count >= 30, $"Solo se encontraron {Ensamblados.Queries.Count} queries.");
        Assert.True(Ensamblados.Handlers.Count >= 80, $"Solo se encontraron {Ensamblados.Handlers.Count} handlers.");
    }

    [Fact]
    public void Todo_request_es_command_o_query()
        => SinViolaciones(Ensamblados.Requests,
            t => Ensamblados.EsCommand(t) == Ensamblados.EsQuery(t),
            "debe implementar ICommand, ICommand<T> o IQuery<T> (solo uno, no IRequest a secas)");

    [Fact]
    public void Commands_y_queries_son_sealed_record()
        => SinViolaciones(Ensamblados.Requests,
            t => !t.IsSealed || !Ensamblados.EsRecord(t),
            "debe declararse como 'public sealed record'");

    [Fact]
    public void Commands_y_queries_son_inmutables()
        => SinViolaciones(Ensamblados.Requests,
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Any(TieneSetMutable),
            "no debe tener propiedades con 'set' (usa parámetros del record o 'init')");

    [Fact]
    public void Commands_se_llaman_Command_y_viven_en_Commands()
        => SinViolaciones(Ensamblados.Commands,
            t => !t.Name.EndsWith("Command", StringComparison.Ordinal) || !EnCarpeta(t, "Commands"),
            "debe llamarse XxxCommand y estar en el namespace ...Commands");

    [Fact]
    public void Queries_se_llaman_Query_y_viven_en_Queries()
        => SinViolaciones(Ensamblados.Queries,
            t => !t.Name.EndsWith("Query", StringComparison.Ordinal) || !EnCarpeta(t, "Queries"),
            "debe llamarse XxxQuery y estar en el namespace ...Queries");

    [Fact]
    public void Cada_request_tiene_exactamente_un_handler()
    {
        var handlersPorRequest = Ensamblados.Handlers.ToLookup(Ensamblados.RequestDe);

        SinViolaciones(Ensamblados.Requests,
            t => handlersPorRequest[t].Count() != 1,
            "debe tener exactamente un handler");
    }

    [Fact]
    public void Handlers_son_sealed_y_se_llaman_como_su_request()
        => SinViolaciones(Ensamblados.Handlers,
            h =>
            {
                var request = Ensamblados.RequestDe(h);
                return !h.IsSealed || h.Name != request.Name + "Handler" || h.Namespace != request.Namespace;
            },
            "debe ser 'public sealed class XxxHandler' en el mismo namespace que su request");

    [Fact]
    public void Handlers_de_commands_implementan_ICommandHandler()
        => SinViolaciones(Ensamblados.Handlers.Where(h => Ensamblados.EsCommand(Ensamblados.RequestDe(h))),
            h => !Ensamblados.InterfacesGenericas(h, typeof(ICommandHandler<>), typeof(ICommandHandler<,>)).Any(),
            "debe implementar ICommandHandler<...> (no IRequestHandler)");

    [Fact]
    public void Handlers_de_queries_implementan_IQueryHandler()
        => SinViolaciones(Ensamblados.Handlers.Where(h => Ensamblados.EsQuery(Ensamblados.RequestDe(h))),
            h => !Ensamblados.InterfacesGenericas(h, typeof(IQueryHandler<,>)).Any(),
            "debe implementar IQueryHandler<...> (no IRequestHandler)");

    [Fact]
    public void Notificaciones_y_sus_handlers_son_sealed()
    {
        SinViolaciones(Ensamblados.Notificaciones,
            t => !t.IsSealed || !Ensamblados.EsRecord(t),
            "debe declararse como 'public sealed record'");
        SinViolaciones(Ensamblados.HandlersDeNotificacion,
            t => !t.IsSealed,
            "debe ser sealed");
    }

    private static bool EnCarpeta(Type t, string carpeta)
        => t.Namespace?.EndsWith("." + carpeta, StringComparison.Ordinal) == true;

    /// <summary>true si la propiedad tiene un 'set' público normal (no 'init').</summary>
    private static bool TieneSetMutable(PropertyInfo propiedad)
    {
        var set = propiedad.SetMethod;
        if (set is null || !set.IsPublic)
            return false;

        var esInit = set.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit));
        return !esInit;
    }

    internal static void SinViolaciones(IEnumerable<Type> tipos, Func<Type, bool> violaLaRegla, string regla)
    {
        var malos = tipos.Where(violaLaRegla).Select(Ensamblados.Nombre).OrderBy(n => n).ToList();

        Assert.True(malos.Count == 0,
            $"{malos.Count} tipo(s) no cumplen la regla: {regla}.\n  - {string.Join("\n  - ", malos)}");
    }
}
