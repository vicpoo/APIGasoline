using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

public class DispositivoEmpleado
{
    public const int TokenMaxLength = 512;

    public Guid DispositivoEmpleadoId { get; private set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; private set; }
    public Guid EmpleadoId { get; private set; }
    public string TokenFcm { get; private set; } = string.Empty;
    public PlataformaDispositivo Plataforma { get; private set; }
    public bool Activo { get; private set; }
    public DateTime FechaRegistro { get; private set; }
    public DateTime? FechaUltimoUso { get; private set; }

    // Requerido por Entity Framework
    private DispositivoEmpleado()
    {
    }

    public static DispositivoEmpleado Registrar(
        Guid empresaId,
        Guid empleadoId,
        string tokenFcm,
        PlataformaDispositivo plataforma,
        DateTime ahora)
    {
        if (ahora == default)
            throw new DomainException("La fecha de registro es obligatoria.");

        var dispositivo = new DispositivoEmpleado { FechaRegistro = ahora };
        dispositivo.Asignar(empresaId, empleadoId, tokenFcm, plataforma, ahora);

        return dispositivo;
    }

    /// <summary>
    /// La app vuelve a mandar un token que ya existe: puede ser el mismo celular
    /// con otro usuario, así que se asigna al empleado actual y se reactiva.
    /// </summary>
    public void Reasignar(Guid empresaId, Guid empleadoId, PlataformaDispositivo plataforma, DateTime ahora)
        => Asignar(empresaId, empleadoId, TokenFcm, plataforma, ahora);

    public void RegistrarUso(DateTime ahora) => FechaUltimoUso = ahora;

    public void Desactivar() => Activo = false;

    private void Asignar(Guid empresaId, Guid empleadoId, string tokenFcm, PlataformaDispositivo plataforma, DateTime ahora)
    {
        if (empresaId == Guid.Empty)
            throw new DomainException("El id de la empresa es obligatorio.");

        if (empleadoId == Guid.Empty)
            throw new DomainException("El id del empleado es obligatorio.");

        var token = tokenFcm?.Trim() ?? string.Empty;

        if (token.Length == 0)
            throw new DomainException("El token del dispositivo es obligatorio.");

        if (token.Length > TokenMaxLength)
            throw new DomainException($"El token del dispositivo no puede exceder {TokenMaxLength} caracteres.");

        if (!Enum.IsDefined(plataforma))
            throw new DomainException("La plataforma del dispositivo no es válida.");

        EmpresaId = empresaId;
        EmpleadoId = empleadoId;
        TokenFcm = token;
        Plataforma = plataforma;
        Activo = true;
        FechaUltimoUso = ahora;
    }
}
