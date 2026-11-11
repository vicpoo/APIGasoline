using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Sesiones;
using APIGasoline.Application.Features.Empleados.Common;
using APIGasoline.Application.Features.Empleados.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Empleados.Commands;

/// <summary>
/// Actualiza los datos del empleado. Las fotos NO se cambian aquí: usa SubirFotoEmpleadoCommand / EliminarFotoEmpleadoCommand.
/// Si cambia de usuario o de empresa, se cierran las sesiones del usuario anterior y del nuevo
/// (sus tokens traen el empleado y la empresa viejos).
/// </summary>
public sealed record UpdateEmpleadoCommand(
    Guid EmpleadoId,
    Guid EmpresaId,
    Guid UsuarioId,
    string NombreCompleto) : ICommand<EmpleadoDto>, IRecursoPropio
{
    /// <summary>El empleado puede editar su nombre, pero no moverse de empresa ni de usuario; con empleado.gestionar, cualquiera.</summary>
    public bool EsPropio(IUsuarioActual usuario)
        => EmpleadoId == usuario.EmpleadoId
        && EmpresaId == usuario.EmpresaId
        && UsuarioId == usuario.UsuarioId;

    public string PrivilegioParaOtros => Permisos.EmpleadoGestionar;
}

public sealed class UpdateEmpleadoCommandHandler : ICommandHandler<UpdateEmpleadoCommand, EmpleadoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IDateTimeProvider _dateTime;

    public UpdateEmpleadoCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IFileStorageService storage,
        IUsuarioActual usuarioActual,
        IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
        _usuarioActual = usuarioActual;
        _dateTime = dateTime;
    }

    public async Task<EmpleadoDto> Handle(UpdateEmpleadoCommand request, CancellationToken cancellationToken)
    {
        var empleado = await _unitOfWork.Empleados.GetByIdAsync(request.EmpleadoId, cancellationToken)
            ?? throw new NotFoundException($"No existe un empleado con el id {request.EmpleadoId}.");

        var usuarioAnterior = empleado.UsuarioId;
        var empresaAnterior = empleado.EmpresaId;

        // El dominio valida y normaliza los datos
        empleado.CambiarEmpresa(request.EmpresaId);
        empleado.CambiarUsuario(request.UsuarioId);
        empleado.CambiarNombreCompleto(request.NombreCompleto);

        await EmpleadoValidator.ValidarAsync(_unitOfWork, empleado.EmpresaId, empleado.UsuarioId, empleado.EmpleadoId, cancellationToken);

        if (usuarioAnterior != empleado.UsuarioId || empresaAnterior != empleado.EmpresaId)
        {
            var ahora = _dateTime.Now;

            await _unitOfWork.RevocarDeUsuarioAsync(
                usuarioAnterior, MotivoRevocacionSesion.EmpleadoReasignado, ahora, _usuarioActual.UsuarioId,
                cancellationToken: cancellationToken);

            if (usuarioAnterior != empleado.UsuarioId)
            {
                await _unitOfWork.RevocarDeUsuarioAsync(
                    empleado.UsuarioId, MotivoRevocacionSesion.EmpleadoReasignado, ahora, _usuarioActual.UsuarioId,
                    cancellationToken: cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _mapper.Map<EmpleadoDto>(empleado).ConUrlsAsync(_storage, cancellationToken);
    }
}
