using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;


public class CentroCosto
{
    public const int NombreMaxLength = 150;
    public const int ClaveMaxLength = 100;

    public int CentroCostoId { get; private set; }
    public int EmpresaId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Clave { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;


    public DateTime? FechaRegistro { get; private set; }

    // Requerido por Entity Framework
    private CentroCosto()
    {
    }

    public static CentroCosto Crear(int empresaId, string nombre, string clave, bool activo = true)
    {
        var centroCosto = new CentroCosto();
        centroCosto.CambiarEmpresa(empresaId);
        centroCosto.CambiarNombre(nombre);
        centroCosto.CambiarClave(clave);
        centroCosto.Activo = activo;

        return centroCosto;
    }

    public void CambiarEmpresa(int empresaId)
    {
        if (empresaId <= 0)
            throw new DomainException("El id de la empresa debe ser mayor a cero.");

        EmpresaId = empresaId;
    }

    public void CambiarNombre(string nombre)
    {
        var normalizado = nombre?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizado))
            throw new DomainException("El nombre del centro de costo es obligatorio.");

        if (normalizado.Length > NombreMaxLength)
            throw new DomainException($"El nombre del centro de costo no puede exceder {NombreMaxLength} caracteres.");

        Nombre = normalizado;
    }

    public void CambiarClave(string clave)
    {
        var normalizada = clave?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizada))
            throw new DomainException("La clave del centro de costo es obligatoria.");

        if (normalizada.Length > ClaveMaxLength)
            throw new DomainException($"La clave del centro de costo no puede exceder {ClaveMaxLength} caracteres.");

        Clave = normalizada;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;
}
