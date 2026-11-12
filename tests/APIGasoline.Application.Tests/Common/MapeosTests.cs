using APIGasoline.Application;
using AutoMapper;
using AutoMapper.Internal;

namespace APIGasoline.Application.Tests.Common;

/// <summary>
/// F016: todos los perfiles de AutoMapper son válidos. Si a un DTO se le agrega una propiedad que
/// ninguna entidad llena (o se renombra una), esta prueba falla en lugar de mandar el campo vacío.
/// </summary>
public class MapeosTests
{
    [Fact]
    public void La_configuracion_de_AutoMapper_es_valida()
    {
        var configuracion = new MapperConfiguration(cfg => cfg.AddMaps(typeof(DependencyInjection).Assembly));

        configuracion.AssertConfigurationIsValid();
    }

    /// <summary>Control: si no se cargaran los perfiles, la prueba de arriba pasaría sin revisar nada.</summary>
    [Fact]
    public void Se_cargan_todos_los_perfiles()
    {
        var configuracion = new MapperConfiguration(cfg => cfg.AddMaps(typeof(DependencyInjection).Assembly));
        var perfiles = typeof(DependencyInjection).Assembly.GetTypes().Count(t => t.IsSubclassOf(typeof(Profile)) && !t.IsAbstract);

        Assert.True(perfiles >= 20, $"Solo se encontraron {perfiles} perfiles.");
        Assert.True(configuracion.Internal().GetAllTypeMaps().Count >= perfiles,
            $"Hay {perfiles} perfiles pero solo {configuracion.Internal().GetAllTypeMaps().Count} mapeos.");
    }
}
