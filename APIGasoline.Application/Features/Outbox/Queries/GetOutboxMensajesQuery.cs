using APIGasoline.Application.Features.Outbox.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Outbox.Queries;

/// <summary>Para revisar qué push se enviaron, cuáles están en reintento y por qué fallaron.</summary>
public record GetOutboxMensajesQuery(Guid? EmpresaId, EstadoOutbox? Estado, int Limite) : IRequest<IReadOnlyList<OutboxMensajeDto>>;

public class GetOutboxMensajesQueryHandler : IRequestHandler<GetOutboxMensajesQuery, IReadOnlyList<OutboxMensajeDto>>
{
    public const int LimiteMaximo = 500;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOutboxMensajesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<OutboxMensajeDto>> Handle(GetOutboxMensajesQuery request, CancellationToken cancellationToken)
    {
        var limite = Math.Clamp(request.Limite, 1, LimiteMaximo);
        var mensajes = await _unitOfWork.OutboxMensajes.GetAllAsync(request.EmpresaId, request.Estado, limite, cancellationToken);

        return _mapper.Map<IReadOnlyList<OutboxMensajeDto>>(mensajes);
    }
}
