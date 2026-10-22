using APIGasoline.Application.Common.Interfaces;

namespace APIGasoline.Application.Common.Security;

/// <summary>
/// Marca un Command/Query que actúa sobre algo "del usuario" (su perfil, su licencia, su contraseña,
/// su celular, sus notificaciones, sus viajes...). <see cref="AccesoPropioBehavior{TRequest,TResponse}"/>
/// lo valida ANTES del handler: pasa si es suyo, o si tiene <see cref="PrivilegioParaOtros"/>.
/// </summary>
public interface IRecursoPropio
{
    /// <summary>true si el registro pertenece al usuario del token.</summary>
    bool EsPropio(IUsuarioActual usuario);

    /// <summary>Privilegio con el que también se puede actuar sobre lo de otros empleados.</summary>
    string PrivilegioParaOtros { get; }
}
