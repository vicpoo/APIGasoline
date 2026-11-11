namespace APIGasoline.Infrastructure.Persistence;

/// <summary>
/// Nombres de los filtros globales de EF Core. Tener nombre permite quitar solo uno
/// (<c>IgnoreQueryFilters([FiltrosConsulta.Eliminado])</c>) sin quitar el otro.
/// </summary>
public static class FiltrosConsulta
{
    /// <summary>Oculta los registros borrados lógicamente (empleados y empresas).</summary>
    public const string Eliminado = "Eliminado";

    /// <summary>
    /// Aislamiento entre empresas: un usuario que no es de la plataforma solo ve lo de su empresa.
    /// Quitarlo SOLO para validar unicidad global (placas, correo...) y nunca para devolver datos.
    /// </summary>
    public const string Empresa = "Empresa";
}
