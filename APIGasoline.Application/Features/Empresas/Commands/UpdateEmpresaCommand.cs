using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Messaging;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Sesiones;
using APIGasoline.Application.Features.Empresas.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;

namespace APIGasoline.Application.Features.Empresas.Commands;

/// <summary>
/// Edita la empresa. Activarla o desactivarla (HU-ADM-01) es cosa de la plataforma
/// (<see cref="Permisos.EmpresaGestionar"/>): una empresa inactiva no puede entrar, así que su
/// propio administrador no debe poder dejarse fuera. Al desactivarla se cierran sus sesiones (F009).
/// </summary>
/// <param name="Activo">Activa o inactiva. null = no cambia.</param>
public sealed record UpdateEmpresaCommand(Guid EmpresaId, string Nombre, bool? Activo = null) : ICommand<EmpresaDto>;

public sealed class UpdateEmpresaCommandHandler : ICommandHandler<UpdateEmpresaCommand, EmpresaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IDateTimeProvider _dateTime;

    public UpdateEmpresaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IUsuarioActual usuarioActual, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usuarioActual = usuarioActual;
        _dateTime = dateTime;
    }

    public async Task<EmpresaDto> Handle(UpdateEmpresaCommand request, CancellationToken cancellationToken)
    {
        var empresa = await _unitOfWork.Empresas.GetByIdAsync(request.EmpresaId, cancellationToken)
            ?? throw new NotFoundException($"No existe una empresa con el id {request.EmpresaId}.");

        var cambiaEstado = request.Activo is { } activo && activo != empresa.Activo;

        if (cambiaEstado && !await _usuarioActual.TienePrivilegioAsync(Permisos.EmpresaGestionar, cancellationToken))
            throw new ForbiddenException($"Solo la plataforma puede activar o desactivar una empresa (privilegio '{Permisos.EmpresaGestionar}').");

        // El dominio valida y normaliza los datos
        empresa.CambiarNombre(request.Nombre);

        if (request.Activo is true)
            empresa.Activar();
        else if (request.Activo is false)
            empresa.Desactivar();

        // Desactivarla corta el acceso de inmediato: se cierran todas sus sesiones
        if (cambiaEstado && !empresa.Activo)
        {
            await _unitOfWork.RevocarDeEmpresaAsync(
                empresa.EmpresaId, MotivoRevocacionSesion.EmpresaDesactivada, _dateTime.Now, _usuarioActual.UsuarioId,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<EmpresaDto>(empresa);
    }
}
