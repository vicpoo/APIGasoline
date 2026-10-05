using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.PuntosRuta.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.PuntosRuta.Queries;

public record GetPuntoRutaByIdQuery(int PuntoRutaId) : IRequest<PuntoRutaDto>;

public class GetPuntoRutaByIdQueryHandler : IRequestHandler<GetPuntoRutaByIdQuery, PuntoRutaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPuntoRutaByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PuntoRutaDto> Handle(GetPuntoRutaByIdQuery request, CancellationToken cancellationToken)
    {
        var puntoRuta = await _unitOfWork.PuntosRuta.GetByIdAsync(request.PuntoRutaId, cancellationToken)
            ?? throw new NotFoundException($"No existe un punto de ruta con el id {request.PuntoRutaId}.");

        return _mapper.Map<PuntoRutaDto>(puntoRuta);
    }
}
