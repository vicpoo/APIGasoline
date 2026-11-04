using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Entities;

namespace APIGasoline.Application.Features.Viajes.Common;

/// <summary>
/// Offline-first: la app genera el UUID del viaje y de cada carga en el teléfono y lo manda al crearlos.
/// Si la respuesta se pierde (sin señal, timeout), la app vuelve a mandar lo mismo con el mismo UUID.
/// Estas reglas deciden si lo que ya existe con ese UUID es ese mismo registro (reintento: se regresa
/// tal cual, sin crear otro) o si el UUID está ocupado por otra cosa (409 ID_DUPLICADO).
/// Es pura (sin BD) para poder probarla.
/// </summary>
public static class ReintentoRegistro
{
    /// <summary>Lanza 409 si el viaje existente no es el mismo (empresa, carro y operador).</summary>
    public static void ValidarViaje(Viaje existente, Guid empresaId, Guid carroId, Guid empleadoOperadorId)
    {
        var esElMismo = existente.EmpresaId == empresaId
            && existente.CarroId == carroId
            && existente.EmpleadoOperadorId == empleadoOperadorId;

        if (!esElMismo)
            throw Duplicado($"El id {existente.ViajeId} ya pertenece a otro viaje.", existente.ViajeId);
    }

    /// <summary>Lanza 409 si la carga existente es de otro viaje.</summary>
    public static void ValidarCarga(CargaCombustible existente, Guid viajeId)
    {
        if (existente.ViajeId != viajeId)
            throw Duplicado($"El id {existente.CargaCombustibleId} ya pertenece a otra carga.", existente.CargaCombustibleId);
    }

    private static ConflictException Duplicado(string mensaje, Guid id)
        => new(mensaje, CodigosConflicto.IdDuplicado, new Dictionary<string, object?> { ["id"] = id });
}
