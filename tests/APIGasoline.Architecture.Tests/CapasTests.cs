using System.Reflection;

namespace APIGasoline.Architecture.Tests;

/// <summary>
/// Clean Architecture: las dependencias solo apuntan hacia adentro.
/// API → Infrastructure → Application → Domain.
/// </summary>
public sealed class CapasTests
{
    [Fact]
    public void Domain_no_depende_de_nadie()
        => SinReferenciasA(Ensamblados.Domain,
            "APIGasoline.Application", "APIGasoline.Infrastructure", "APIGasoline",
            "MediatR", "AutoMapper", "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore");

    [Fact]
    public void Application_no_depende_de_Infrastructure_ni_de_la_API()
        => SinReferenciasA(Ensamblados.Application,
            "APIGasoline.Infrastructure", "APIGasoline",
            "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore");

    [Fact]
    public void Infrastructure_no_depende_de_la_API()
        => SinReferenciasA(Ensamblados.Infrastructure, "APIGasoline");

    private static void SinReferenciasA(Assembly ensamblado, params string[] prohibidos)
    {
        var referencias = ensamblado.GetReferencedAssemblies().Select(r => r.Name ?? "").ToList();

        // "APIGasoline" (la API) se compara exacto; los demás como prefijo (Microsoft.EntityFrameworkCore.*)
        var malas = referencias
            .Where(r => prohibidos.Any(p => r == p || (p != "APIGasoline" && r.StartsWith(p + ".", StringComparison.Ordinal))))
            .ToList();

        Assert.True(malas.Count == 0,
            $"{ensamblado.GetName().Name} no debe depender de: {string.Join(", ", malas)}");
    }
}
