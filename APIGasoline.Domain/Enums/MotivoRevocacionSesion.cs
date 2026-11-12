namespace APIGasoline.Domain.Enums;

/// <summary>Por qué se cerró (revocó) una sesión. Se guarda en sesion_usuario.motivo_revocacion.</summary>
public enum MotivoRevocacionSesion
{
    /// <summary>El usuario cerró sesión en ese dispositivo (POST /api/auth/logout).</summary>
    CierreSesion = 0,

    /// <summary>El usuario la cerró desde su lista de sesiones abiertas.</summary>
    CerradaPorUsuario = 1,

    /// <summary>Un administrador (usuario.gestionar) la cerró.</summary>
    CerradaPorAdministrador = 2,

    /// <summary>Se cambió la contraseña del usuario.</summary>
    CambioPassword = 3,

    /// <summary>El usuario se desactivó.</summary>
    UsuarioDesactivado = 4,

    /// <summary>El empleado ligado al usuario se dio de baja.</summary>
    EmpleadoDadoDeBaja = 5,

    /// <summary>El empleado se ligó a otro usuario: el anterior ya no tiene empleado.</summary>
    EmpleadoReasignado = 6,

    /// <summary>La empresa del empleado se dio de baja.</summary>
    EmpresaEliminada = 7,

    /// <summary>
    /// Se usó un refresh token que ya había sido reemplazado: alguien pudo robarlo,
    /// así que se cierra la sesión completa (el dueño y el atacante tienen que volver a entrar).
    /// </summary>
    ReusoDetectado = 8,

    /// <summary>Al renovar, el usuario ya no puede entrar (inactivo, sin empleado, empresa eliminada...).</summary>
    AccesoRevocado = 9,

    /// <summary>El usuario superó el máximo de sesiones abiertas: se cierra la más vieja.</summary>
    LimiteSesiones = 10,

    /// <summary>La empresa se marcó como inactiva (F009): sus sesiones se cierran y no puede volver a entrar.</summary>
    EmpresaDesactivada = 11
}
