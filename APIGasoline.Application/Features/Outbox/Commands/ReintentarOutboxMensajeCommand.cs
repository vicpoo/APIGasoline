using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Outbox.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Outbox.Commands;

/// <summary>Vuelve a poner en cola un mensaje fallido (el worker lo toma en la siguiente vuelta).</summary>
public record ReintentarOutboxMensajeCommand(Guid OutboxMensajeId) : IRequest<OutboxMensajeDto>;

public class ReintentarOutboxMensajeCommandHandler : IRequestHandler<ReintentarOutboxMensajeCommand, OutboxMensajeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;

    public ReintentarOutboxMensajeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
    }

    public async Task<OutboxMensajeDto> Handle(ReintentarOutboxMensajeCommand request, CancellationToken cancellationToken)
    {
        var mensaje = await _unitOfWork.OutboxMensajes.GetByIdAsync(request.OutboxMensajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un mensaje del outbox con el id {request.OutboxMensajeId}.");

        mensaje.Reintentar(_dateTime.Now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<OutboxMensajeDto>(mensaje);
    }
}
