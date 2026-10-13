using APIGasoline.Application.Common.Auditoria;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.RutasProcesadas.Notifications;
using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Viajes.Commands;

/// <summary>
/// Corrige el kilometraje de un viaje capturado por error.
/// - Viaje en curso: lo corrige el operador (o un Confirmador / Gestor de activos).
/// - Viaje cerrado: solo un Confirmador o Gestor de activos.
/// El motivo es obligatorio. El viaje queda marcado como corregido y el valor anterior
/// se conserva en auditoria_evento. Si el viaje ya estaba cerrado, se recalcula su ruta procesada.
/// </summary>
public record CorregirKilometrajeViajeCommand(
    Guid ViajeId,
    decimal KilometrajeInicial,
    decimal? KilometrajeFinal,
    string? Motivo) : IRequest<ViajeDto>;

public class CorregirKilometrajeViajeCommandHandler : IRequestHandler<CorregirKilometrajeViajeCommand, ViajeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;
    private readonly IPublisher _publisher;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IContextoAuditoria _auditoria;

    public CorregirKilometrajeViajeCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IFileStorageService storage,
        IPublisher publisher,
        IUsuarioActual usuarioActual,
        IContextoAuditoria auditoria)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
        _publisher = publisher;
        _usuarioActual = usuarioActual;
        _auditoria = auditoria;
    }

    public async Task<ViajeDto> Handle(CorregirKilometrajeViajeCommand request, CancellationToken cancellationToken)
    {
        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        var motivo = ReglasCorreccion.Validar(_usuarioActual, viaje, request.Motivo);

        var carro = await _unitOfWork.Carros.GetByIdAsync(viaje.CarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un carro con el id {viaje.CarroId}.");

        var kilometrajeFinalAnterior = viaje.KilometrajeFinal;

        // El dominio valida los valores y recalcula la distancia
        viaje.CorregirKilometraje(request.KilometrajeInicial, request.KilometrajeFinal);

        // Si este viaje fue el que dejó el kilometraje actual del carro, se ajusta al valor corregido
        if (viaje.KilometrajeFinal is { } kilometrajeFinal)
        {
            if (kilometrajeFinalAnterior is not null && carro.KilometrajeActual == kilometrajeFinalAnterior)
                carro.CorregirKilometraje(kilometrajeFinal);
            else
                carro.AvanzarKilometraje(kilometrajeFinal);
        }

        // El interceptor guarda el valor anterior y el nuevo con esta acción y motivo
        _auditoria.Registrar(viaje.ViajeId, AccionAuditoria.Corregir, motivo);
        _auditoria.Registrar(carro.CarroId, AccionAuditoria.Corregir, $"Corrección del viaje {viaje.ViajeId}: {motivo}");

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // El kilometraje cambia la comparación odómetro contra GPS de un viaje ya cerrado
        if (!viaje.EstaEnCurso)
            await _publisher.Publish(new RutaViajeModificadaNotification(viaje.ViajeId), cancellationToken);

        return await _mapper.Map<ViajeDto>(viaje).ConUrlsAsync(_storage, cancellationToken);
    }
}
