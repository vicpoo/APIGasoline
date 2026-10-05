using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// Bitácora de cambios de estado realizados sobre un carro.
/// Es inmutable: se crea junto con el cambio de estado y no se edita ni se borra.
/// </summary>
public class CambioEstatusCarro
{
    public const int MotivoMaxLength = 255;

    public int CambioEstatusCarroId { get; private set; }
    public int EmpresaId { get; private set; }
    public int CarroId { get; private set; }
    public int EmpleadoId { get; private set; }
    public int? ViajeAfectadoId { get; private set; }
    public EstadoCarro EstatusAnterior { get; private set; }
    public EstadoCarro EstatusNuevo { get; private set; }
    public string? Motivo { get; private set; }

    /// <summary>
    /// Si la app la manda (registro offline) se usa esa; si no, la pone la BD (default now()).
    /// </summary>
    public DateTime? FechaCambio { get; private set; }

    /// <summary>true cuando el cambio se registró sin conexión y se subió después.</summary>
    public bool Sincronizado { get; private set; }

    // Requerido por Entity Framework
    private CambioEstatusCarro()
    {
    }

    /// <summary>
    /// Cambia el estado del carro y devuelve el registro de bitácora correspondiente.
    /// Es la única forma de cambiar el estado de un carro, así nunca queda un cambio sin registrar.
    /// </summary>
    public static CambioEstatusCarro Registrar(
        Carro carro,
        int empleadoId,
        EstadoCarro estatusNuevo,
        string? motivo,
        int? viajeAfectadoId = null,
        DateTime? fechaCambio = null,
        bool sincronizado = false)
    {
        ArgumentNullException.ThrowIfNull(carro);

        if (empleadoId <= 0)
            throw new DomainException("El id del empleado debe ser mayor a cero.");

        if (viajeAfectadoId is <= 0)
            throw new DomainException("El id del viaje afectado debe ser mayor a cero.");

        var motivoNormalizado = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();

        if (motivoNormalizado is not null && motivoNormalizado.Length > MotivoMaxLength)
            throw new DomainException($"El motivo no puede exceder {MotivoMaxLength} caracteres.");

        if (estatusNuevo == EstadoCarro.EnMantenimiento && motivoNormalizado is null)
            throw new DomainException("El motivo es obligatorio para mandar un carro a mantenimiento.");

        var estatusAnterior = carro.Estado;

        if (estatusAnterior == estatusNuevo)
            throw new DomainException($"El carro ya se encuentra en estado {estatusNuevo}.");

        // Cada transición usa la regla del carro
        switch (estatusNuevo)
        {
            case EstadoCarro.Ocupado:
                carro.Ocupar();
                break;
            default:
                carro.CambiarEstado(estatusNuevo);
                break;
        }

        return new CambioEstatusCarro
        {
            EmpresaId = carro.EmpresaId,
            CarroId = carro.CarroId,
            EmpleadoId = empleadoId,
            ViajeAfectadoId = viajeAfectadoId,
            EstatusAnterior = estatusAnterior,
            EstatusNuevo = estatusNuevo,
            Motivo = motivoNormalizado,
            FechaCambio = fechaCambio,
            Sincronizado = sincronizado
        };
    }
}
