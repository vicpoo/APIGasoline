using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

public class CambioEstatusCarro
{
    public const int MotivoMaxLength = 255;
    public const int FotoMaxLength = 255;

    public Guid CambioEstatusCarroId { get; private set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; private set; }
    public Guid CarroId { get; private set; }
    public Guid EmpleadoId { get; private set; }
    public Guid? ViajeAfectadoId { get; private set; }
    public EstadoCarro EstatusAnterior { get; private set; }
    public EstadoCarro EstatusNuevo { get; private set; }
    /// <summary>
    /// Clasificación del cambio. Con FallaMecanica y ChoqueOGolpe se obtiene el registro de percances.
    /// </summary>
    public TipoMotivoCambio? TipoMotivo { get; private set; }

    public string? Motivo { get; private set; }

    /// <summary>Llave en MinIO de la foto opcional de la falla o del daño (HU-EST-02).</summary>
    public string? FotoEvidencia { get; private set; }

    public DateTime? FechaCambio { get; private set; }

    /// <summary>Hora del servidor en que llegó el registro (la pone la BD). Reemplaza al antiguo campo "sincronizado".</summary>
    public DateTime? FechaRecepcion { get; private set; }

    // Requerido por Entity Framework
    private CambioEstatusCarro()
    {
    }

    /// <summary>
    /// Agrega (o reemplaza) la foto de evidencia. Se sube después de registrar el cambio,
    /// porque desde el celular puede llegar más tarde que los datos (offline).
    /// </summary>
    public void CambiarFotoEvidencia(string? llave)
    {
        var normalizada = string.IsNullOrWhiteSpace(llave) ? null : llave.Trim();

        if (normalizada is not null && normalizada.Length > FotoMaxLength)
            throw new DomainException($"La ruta de la foto de evidencia no puede exceder {FotoMaxLength} caracteres.");

        FotoEvidencia = normalizada;
    }

    public static CambioEstatusCarro Registrar(
        Carro carro,
        Guid empleadoId,
        EstadoCarro estatusNuevo,
        string? motivo,
        Guid? viajeAfectadoId = null,
        DateTime? fechaCambio = null,
        TipoMotivoCambio? tipoMotivo = null,
        string? fotoEvidencia = null)
    {
        ArgumentNullException.ThrowIfNull(carro);

        if (carro.DadoDeBaja)
            throw new DomainException("El carro está dado de baja; no se puede cambiar su estatus.");

        if (empleadoId == Guid.Empty)
            throw new DomainException("El id del empleado es obligatorio.");

        if (viajeAfectadoId == Guid.Empty)
            throw new DomainException("El id del viaje afectado es obligatorio.");

        var motivoNormalizado = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();

        if (motivoNormalizado is not null && motivoNormalizado.Length > MotivoMaxLength)
            throw new DomainException($"El motivo no puede exceder {MotivoMaxLength} caracteres.");

        if (tipoMotivo is not null && !Enum.IsDefined(tipoMotivo.Value))
            throw new DomainException("El tipo de motivo no es válido.");

        var fotoNormalizada = string.IsNullOrWhiteSpace(fotoEvidencia) ? null : fotoEvidencia.Trim();

        if (fotoNormalizada is not null && fotoNormalizada.Length > FotoMaxLength)
            throw new DomainException($"La ruta de la foto de evidencia no puede exceder {FotoMaxLength} caracteres.");

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
            TipoMotivo = tipoMotivo,
            Motivo = motivoNormalizado,
            FotoEvidencia = fotoNormalizada,
            FechaCambio = fechaCambio
        };
    }
}
