using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Notificaciones.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Notificaciones.Queries;

public record GetNotificacionByIdQuery(Guid NotificacionId) : IRequest<NotificacionDto>;

public class GetNotificacionByIdQueryHandler : IRequestHandler<GetNotificacionByIdQuery, NotificacionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetNotificacionByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<NotificacionDto> Handle(GetNotificacionByIdQuery request, CancellationToken cancellationToken)
    {
        var notificacion = await _unitOfWork.Notificaciones.GetByIdAsync(request.NotificacionId, cancellationToken)
            ?? throw new NotFoundException($"No existe una notificación con el id {request.NotificacionId}.");

        return _mapper.Map<NotificacionDto>(notificacion);
    }
}
