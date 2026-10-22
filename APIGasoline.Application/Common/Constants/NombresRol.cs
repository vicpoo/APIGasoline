namespace APIGasoline.Application.Common.Constants;

/// <summary>
/// Nombres de los roles BASE que crea la siembra inicial (SembrarSeguridadCommand).
///
/// IMPORTANTE: la lógica de negocio y la autorización NUNCA deciden por el nombre de un rol;
/// solo por sus privilegios (<see cref="Security.Permisos"/>). Cada empresa puede renombrar
/// o crear sus propios roles ("Jefe de flota" en vez de "Confirmador") con los privilegios que quiera.
/// </summary>
public static class NombresRol
{
    /// <summary>Rol global con TODOS los privilegios del catálogo (personal de la desarrolladora).</summary>
    public const string AdministradorPlataforma = "Administrador";
}
