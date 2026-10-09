using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

public class CambioEstatusCarro
{
    public const int MotivoMaxLength = 255;

    public Guid CambioEstatusCarroId { get; private set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; private set; }
    public Guid CarroId { get; private set; }
    public Guid EmpleadoId { get; private set; }
    public Guid? ViajeAfectadoId { get; private set; }
    public EstadoCarro EstatusAnterior { get; private set; }
    public EstadoCarro EstatusNuevo { get; private set; }
    public string? Motivo { get; private set; }

    public DateTime? FechaCambio { get; private set; }

    public bool Sincronizado { get; private set; }

    // Requerido por Entity Framework
    private CambioEstatusCarro()
    {
    }

    public static CambioEstatusCarro Registrar(
        Carro carro,
        Guid empleadoId,
        EstadoCarro estatusNuevo,
        string? motivo,
        Guid? viajeAfectadoId = null,
        DateTime? fechaCambio = null,
        bool sincronizado = false)
    {
        ArgumentNullException.ThrowIfNull(carro);

        if (empleadoId == Guid.Empty)
            throw new DomainException("El id del empleado es obligatorio.");

        if (viajeAfectadoId == Guid.Empty)
            throw new DomainException("El id del viaje afectado es obligatorio.");

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
