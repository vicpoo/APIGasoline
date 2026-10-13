using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;


public class Empleado
{
    public const int NombreCompletoMaxLength = 150;
    public const int FotoMaxLength = 255;

    public Guid EmpleadoId { get; private set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string NombreCompleto { get; private set; } = string.Empty;

    public string? FotoPerfil { get; private set; }

    public string? FotoLicencia { get; private set; }

    /// <summary>Cuándo se subió la licencia (HU-LOG-01). null = licencia pendiente.</summary>
    public DateTime? FechaCargaLicencia { get; private set; }

    public bool Eliminado { get; private set; }


    public DateTime? FechaAlta { get; private set; }

    // Requerido por Entity Framework
    private Empleado()
    {
    }

    public static Empleado Crear(Guid empresaId, Guid usuarioId, string nombreCompleto, string? fotoPerfil = null, string? fotoLicencia = null)
    {
        var empleado = new Empleado();
        empleado.CambiarEmpresa(empresaId);
        empleado.CambiarUsuario(usuarioId);
        empleado.CambiarNombreCompleto(nombreCompleto);
        empleado.CambiarFotoPerfil(fotoPerfil);
        empleado.CambiarFotoLicencia(fotoLicencia);

        return empleado;
    }

    public void CambiarEmpresa(Guid empresaId)
    {
        if (empresaId == Guid.Empty)
            throw new DomainException("El id de la empresa es obligatorio.");

        EmpresaId = empresaId;
    }

    public void CambiarUsuario(Guid usuarioId)
    {
        if (usuarioId == Guid.Empty)
            throw new DomainException("El id del usuario es obligatorio.");

        UsuarioId = usuarioId;
    }

    public void CambiarNombreCompleto(string nombreCompleto)
    {
        var normalizado = nombreCompleto?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizado))
            throw new DomainException("El nombre completo del empleado es obligatorio.");

        if (normalizado.Length > NombreCompletoMaxLength)
            throw new DomainException($"El nombre completo no puede exceder {NombreCompletoMaxLength} caracteres.");

        NombreCompleto = normalizado;
    }

    public void CambiarFotoPerfil(string? fotoPerfil)
        => FotoPerfil = NormalizarFoto(fotoPerfil, "foto de perfil");

    /// <param name="fechaCarga">
    /// Cuándo se subió la licencia. Si se quita la licencia, la fecha se borra;
    /// si se cambia sin indicar fecha, se conserva la anterior.
    /// </param>
    public void CambiarFotoLicencia(string? fotoLicencia, DateTime? fechaCarga = null)
    {
        FotoLicencia = NormalizarFoto(fotoLicencia, "foto de licencia");

        FechaCargaLicencia = FotoLicencia is null ? null : fechaCarga ?? FechaCargaLicencia;
    }


    public bool TieneLicencia => !string.IsNullOrWhiteSpace(FotoLicencia);

    public void Eliminar()
    {
        if (Eliminado)
            throw new DomainException("El empleado ya está eliminado.");

        Eliminado = true;
    }

    private static string? NormalizarFoto(string? valor, string campo)
    {
        var normalizado = string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        if (normalizado is not null && normalizado.Length > FotoMaxLength)
            throw new DomainException($"La ruta de la {campo} no puede exceder {FotoMaxLength} caracteres.");

        return normalizado;
    }
}
