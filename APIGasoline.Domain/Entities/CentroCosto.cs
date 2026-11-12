using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;


public class CentroCosto
{
    public const int NombreMaxLength = 150;
    public const int ClaveMaxLength = 100;

    public Guid CentroCostoId { get; private set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; private set; }

    /// <summary>Corporativo de la misma empresa (opcional).</summary>
    public Guid? CorporativoId { get; private set; }

    /// <summary>División de la misma empresa (opcional). Si también hay corporativo, deben estar ligados.</summary>
    public Guid? DivisionId { get; private set; }

    public string Nombre { get; private set; } = string.Empty;
    public string Clave { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;


    public DateTime? FechaRegistro { get; private set; }

    // Requerido por Entity Framework
    private CentroCosto()
    {
    }

    public static CentroCosto Crear(Guid empresaId, string nombre, string clave, bool activo = true, Guid? corporativoId = null, Guid? divisionId = null)
    {
        var centroCosto = new CentroCosto();
        centroCosto.CambiarEmpresa(empresaId);
        centroCosto.CambiarNombre(nombre);
        centroCosto.CambiarClave(clave);
        centroCosto.AsignarOrganizacion(corporativoId, divisionId);
        centroCosto.Activo = activo;

        return centroCosto;
    }

    public void CambiarEmpresa(Guid empresaId)
    {
        if (empresaId == Guid.Empty)
            throw new DomainException("El id de la empresa es obligatorio.");

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

    /// <summary>Corporativo y división del centro (ambos opcionales). Que sean de su empresa y estén ligados lo revisa la aplicación.</summary>
    public void AsignarOrganizacion(Guid? corporativoId, Guid? divisionId)
    {
        if (corporativoId == Guid.Empty)
            throw new DomainException("El id del corporativo no es válido.");

        if (divisionId == Guid.Empty)
            throw new DomainException("El id de la división no es válido.");

        CorporativoId = corporativoId;
        DivisionId = divisionId;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;
}
