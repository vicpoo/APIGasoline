using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;


public class Empleado
{
    public const int NombreCompletoMaxLength = 150;
    public const int FotoMaxLength = 255;

    public int EmpleadoId { get; private set; }
    public int EmpresaId { get; private set; }
    public int UsuarioId { get; private set; }
    public string NombreCompleto { get; private set; } = string.Empty;

    public string? FotoPerfil { get; private set; }

    public string? FotoLicencia { get; private set; }

    public bool Eliminado { get; private set; }


    public DateTime? FechaAlta { get; private set; }

    // Requerido por Entity Framework
    private Empleado()
    {
    }

    public static Empleado Crear(int empresaId, int usuarioId, string nombreCompleto, string? fotoPerfil = null, string? fotoLicencia = null)
    {
        var empleado = new Empleado();
        empleado.CambiarEmpresa(empresaId);
        empleado.CambiarUsuario(usuarioId);
        empleado.CambiarNombreCompleto(nombreCompleto);
        empleado.CambiarFotoPerfil(fotoPerfil);
        empleado.CambiarFotoLicencia(fotoLicencia);

        return empleado;
    }

    public void CambiarEmpresa(int empresaId)
    {
        if (empresaId <= 0)
            throw new DomainException("El id de la empresa debe ser mayor a cero.");

        EmpresaId = empresaId;
    }

    public void CambiarUsuario(int usuarioId)
    {
        if (usuarioId <= 0)
            throw new DomainException("El id del usuario debe ser mayor a cero.");

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

    public void CambiarFotoLicencia(string? fotoLicencia)
        => FotoLicencia = NormalizarFoto(fotoLicencia, "foto de licencia");


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
