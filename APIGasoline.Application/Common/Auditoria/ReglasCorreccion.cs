using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Domain.Entities;

namespace APIGasoline.Application.Common.Auditoria;

/// <summary>
/// Quién puede corregir un dato ya enviado al servidor (kilometraje, litros, monto, comprobante):
/// - Viaje en curso: el operador del viaje, o quien tenga <see cref="Permisos.ViajeCorregirCerrado"/>.
/// - Viaje cerrado: solo quien tenga <see cref="Permisos.ViajeCorregirCerrado"/>.
/// Siempre con motivo. Se decide por PRIVILEGIO, no por nombre de rol: cada empresa puede
/// llamar a ese rol como quiera. Antes de sincronizar, el operador corrige libremente en el
/// celular (el dato aún no existe en el servidor), así que esa parte no pasa por aquí.
/// </summary>
public static class ReglasCorreccion
{
    public const int MotivoMaxLength = 500;

    public static async Task<string> ValidarAsync(
        IUsuarioActual usuario,
        Viaje viaje,
        string? motivo,
        CancellationToken cancellationToken = default)
    {
        var motivoNormalizado = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();

        if (motivoNormalizado is null)
            throw new BadRequestException("El motivo de la corrección es obligatorio.");

        if (motivoNormalizado.Length > MotivoMaxLength)
            throw new BadRequestException($"El motivo no puede exceder {MotivoMaxLength} caracteres.");

        if (!usuario.EstaAutenticado)
            throw new UnauthorizedException("Debes iniciar sesión para corregir un registro.");

        // Un empleado solo corrige registros de su propia empresa (salvo plataforma.multiempresa)
        if (!await usuario.TienePrivilegioAsync(Permisos.PlataformaMultiempresa, cancellationToken)
            && usuario.EmpresaId != viaje.EmpresaId)
            throw new ForbiddenException("No puedes corregir registros de otra empresa.");

        if (await usuario.TienePrivilegioAsync(Permisos.ViajeCorregirCerrado, cancellationToken))
            return motivoNormalizado;

        if (!viaje.EstaEnCurso)
            throw new ForbiddenException(
                $"El viaje ya está cerrado: para corregirlo necesitas el privilegio '{Permisos.ViajeCorregirCerrado}'.");

        if (usuario.EmpleadoId != viaje.EmpleadoOperadorId)
            throw new ForbiddenException("Solo el operador del viaje puede corregir sus datos mientras está en curso.");

        return motivoNormalizado;
    }
}
