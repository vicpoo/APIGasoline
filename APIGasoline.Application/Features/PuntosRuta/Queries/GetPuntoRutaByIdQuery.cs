using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.PuntosRuta.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.PuntosRuta.Queries;

public sealed record GetPuntoRutaByIdQuery(Guid PuntoRutaId) : IQuery<PuntoRutaDto>;

public sealed class GetPuntoRutaByIdQueryHandler : IQueryHandler<GetPuntoRutaByIdQuery, PuntoRutaDto>
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
