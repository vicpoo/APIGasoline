using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Dispositivos.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Dispositivos.Commands;

/// <summary>
/// La app lo llama al iniciar sesión y cada vez que Firebase le renueva el token.
/// Si el token ya existe se reasigna al empleado (mismo celular, otro usuario) en lugar de duplicarlo.
/// </summary>
public sealed record RegistrarDispositivoCommand(
    Guid EmpleadoId,
    string TokenFcm,
    PlataformaDispositivo Plataforma) : ICommand<DispositivoDto>, IRecursoPropio
{
    /// <summary>Cada quien registra su celular; para otro empleado se necesita empleado.gestionar.</summary>
    public bool EsPropio(IUsuarioActual usuario)
        => EmpleadoId == usuario.EmpleadoId;

    public string PrivilegioParaOtros => Permisos.EmpleadoGestionar;
}

public sealed class RegistrarDispositivoCommandHandler : ICommandHandler<RegistrarDispositivoCommand, DispositivoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;

    public RegistrarDispositivoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
    }

    public async Task<DispositivoDto> Handle(RegistrarDispositivoCommand request, CancellationToken cancellationToken)
    {
        var empleado = await _unitOfWork.Empleados.GetByIdAsync(request.EmpleadoId, cancellationToken)
            ?? throw new NotFoundException($"No existe un empleado con el id {request.EmpleadoId}.");

        var ahora = _dateTime.Now;
        var existente = await _unitOfWork.Dispositivos.GetByTokenAsync(request.TokenFcm.Trim(), cancellationToken);

        DispositivoEmpleado dispositivo;

        if (existente is null)
        {
            // El celular sigue registrado a un empleado de otra empresa: no se le puede quitar desde aquí
            if (await _unitOfWork.Dispositivos.TokenRegistradoAsync(request.TokenFcm.Trim(), cancellationToken))
                throw new ConflictException("Este dispositivo ya está registrado a un empleado de otra empresa.");

            dispositivo = DispositivoEmpleado.Registrar(empleado.EmpresaId, empleado.EmpleadoId, request.TokenFcm, request.Plataforma, ahora);
            await _unitOfWork.Dispositivos.AddAsync(dispositivo, cancellationToken);
        }
        else
        {
            existente.Reasignar(empleado.EmpresaId, empleado.EmpleadoId, request.Plataforma, ahora);
            dispositivo = existente;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<DispositivoDto>(dispositivo);
    }
}
