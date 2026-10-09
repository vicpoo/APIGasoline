using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.SolicitudesReserva.Queries;

public record GetSolicitudReservaByIdQuery(Guid SolicitudReservaId) : IRequest<SolicitudReservaDto>;

public class GetSolicitudReservaByIdQueryHandler : IRequestHandler<GetSolicitudReservaByIdQuery, SolicitudReservaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSolicitudReservaByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SolicitudReservaDto> Handle(GetSolicitudReservaByIdQuery request, CancellationToken cancellationToken)
    {
        var solicitud = await _unitOfWork.SolicitudesReserva.GetByIdAsync(request.SolicitudReservaId, cancellationToken)
            ?? throw new NotFoundException($"No existe una solicitud de reserva con el id {request.SolicitudReservaId}.");

        return _mapper.Map<SolicitudReservaDto>(solicitud);
    }
}
