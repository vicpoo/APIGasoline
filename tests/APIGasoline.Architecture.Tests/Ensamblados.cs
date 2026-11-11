using System.Reflection;
using APIGasoline.Application.Common.Messaging;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Architecture.Tests;

/// <summary>Los ensamblados de cada capa y los tipos que las reglas revisan.</summary>
internal static class Ensamblados
{
    public static readonly Assembly Domain = typeof(IUnitOfWork).Assembly;
    public static readonly Assembly Application = typeof(ICommand).Assembly;
    public static readonly Assembly Infrastructure = typeof(APIGasoline.Infrastructure.DependencyInjection).Assembly;
    public static readonly Assembly Api = typeof(Program).Assembly;

    /// <summary>Clases y records concretos de Application (sin los que genera el compilador).</summary>
    public static IReadOnlyList<Type> TiposDeApplication { get; } = Application.GetTypes()
        .Where(t => t is { IsClass: true, IsAbstract: false } && !EsGeneradoPorCompilador(t))
        .ToList();

    /// <summary>Todo lo que se puede mandar con ISender.Send (Commands y Queries).</summary>
    public static IReadOnlyList<Type> Requests { get; } = TiposDeApplication
        .Where(t => typeof(IBaseRequest).IsAssignableFrom(t))
        .ToList();

    public static IReadOnlyList<Type> Commands { get; } = Requests.Where(EsCommand).ToList();

    public static IReadOnlyList<Type> Queries { get; } = Requests.Where(EsQuery).ToList();

    /// <summary>Handlers de Commands y Queries (los que implementan IRequestHandler).</summary>
    public static IReadOnlyList<Type> Handlers { get; } = TiposDeApplication
        .Where(t => InterfacesGenericas(t, typeof(IRequestHandler<>), typeof(IRequestHandler<,>)).Any())
        .ToList();

    public static IReadOnlyList<Type> Notificaciones { get; } = TiposDeApplication
        .Where(t => typeof(INotification).IsAssignableFrom(t))
        .ToList();

    public static IReadOnlyList<Type> HandlersDeNotificacion { get; } = TiposDeApplication
        .Where(t => InterfacesGenericas(t, typeof(INotificationHandler<>)).Any())
        .ToList();

    public static bool EsCommand(Type t)
        => typeof(ICommand).IsAssignableFrom(t) || InterfacesGenericas(t, typeof(ICommand<>)).Any();

    public static bool EsQuery(Type t) => InterfacesGenericas(t, typeof(IQuery<>)).Any();

    /// <summary>El request que atiende un handler (el primer argumento de IRequestHandler).</summary>
    public static Type RequestDe(Type handler)
        => InterfacesGenericas(handler, typeof(IRequestHandler<>), typeof(IRequestHandler<,>))
            .Select(i => i.GetGenericArguments()[0])
            .Distinct()
            .Single();

    public static IEnumerable<Type> InterfacesGenericas(Type tipo, params Type[] definiciones)
        => tipo.GetInterfaces().Where(i => i.IsGenericType && definiciones.Contains(i.GetGenericTypeDefinition()));

    /// <summary>Un record de C# trae el método <c>&lt;Clone&gt;$</c> que genera el compilador.</summary>
    public static bool EsRecord(Type t) => t.GetMethod("<Clone>$") is not null;

    public static string Nombre(Type t) => t.FullName ?? t.Name;

    private static bool EsGeneradoPorCompilador(Type t)
        => t.Name.Contains('<') || t.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false);
}
