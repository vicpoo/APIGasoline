using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace APIGasoline.Architecture.Tests;

/// <summary>
/// Los controladores solo traducen HTTP ↔ Commands/Queries: reciben únicamente ISender.
/// Así toda regla de negocio y de seguridad (incluido el filtro por empresa) pasa por Application.
/// </summary>
public sealed class ControladoresTests
{
    private static readonly IReadOnlyList<Type> Controladores = Ensamblados.Api.GetTypes()
        .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ControllerBase).IsAssignableFrom(t))
        .ToList();

    [Fact]
    public void Se_encontraron_los_controladores()
        => Assert.True(Controladores.Count >= 20, $"Solo se encontraron {Controladores.Count} controladores.");

    [Fact]
    public void Los_controladores_solo_dependen_de_ISender()
        => CommandsYQueriesTests.SinViolaciones(Controladores,
            c => c.GetConstructors().SelectMany(ctor => ctor.GetParameters()).Any(p => p.ParameterType != typeof(ISender)),
            "debe recibir solo ISender en su constructor (nada de IUnitOfWork, repositorios ni IMediator)");
}
