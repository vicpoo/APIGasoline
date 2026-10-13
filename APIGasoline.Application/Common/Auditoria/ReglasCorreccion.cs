using APIGasoline.Application.Common.Constants;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Entities;

namespace APIGasoline.Application.Common.Auditoria;

/// <summary>
/// Quién puede corregir un dato ya enviado al servidor (kilometraje, litros, monto, comprobante):
/// - Viaje en curso: el operador del viaje, o un Confirmador / Gestor de activos.
/// - Viaje cerrado: solo un Confirmador o Gestor de activos (o el Administrador).
/// Siempre con motivo. Antes de sincronizar, el operador corrige libremente en el celular
/// (el dato aún no existe en el servidor), así que esa parte no pasa por aquí.
/// </summary>
public static class ReglasCorreccion
{
    public const int MotivoMaxLength = 500;

    public static string Validar(IUsuarioActual usuario, Viaje viaje, string? motivo)
    {
        var motivoNormalizado = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();

        if (motivoNormalizado is null)
            throw new BadRequestException("El motivo de la corrección es obligatorio.");

        if (motivoNormalizado.Length > MotivoMaxLength)
            throw new BadRequestException($"El motivo no puede exceder {MotivoMaxLength} caracteres.");

        if (!usuario.EstaAutenticado)
            throw new UnauthorizedException("Debes iniciar sesión para corregir un registro.");

        var esAdministrador = usuario.TieneRol(NombresRol.Administrador);

        // Un empleado solo corrige registros de su propia empresa
        if (!esAdministrador && usuario.EmpresaId != viaje.EmpresaId)
            throw new ForbiddenException("No puedes corregir registros de otra empresa.");

        var esSupervisor = esAdministrador
            || usuario.TieneRol(NombresRol.Confirmador)
            || usuario.TieneRol(NombresRol.GestorActivos);

        if (esSupervisor)
            return motivoNormalizado;

        if (!viaje.EstaEnCurso)
            throw new ForbiddenException("El viaje ya está cerrado: solo un Confirmador o un Gestor de activos puede corregirlo.");

        if (usuario.EmpleadoId != viaje.EmpleadoOperadorId)
            throw new ForbiddenException("Solo el operador del viaje puede corregir sus datos mientras está en curso.");

        return motivoNormalizado;
    }
}
