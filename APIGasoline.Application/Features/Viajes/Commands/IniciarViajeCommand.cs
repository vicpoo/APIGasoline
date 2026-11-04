using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Viajes.Commands;

/// <summary>
/// Inicia un viaje en línea (botón "Iniciar viaje" de la app).
/// Idempotente: si llega <see cref="ViajeId"/> (UUID generado en el teléfono) y ya existe ese mismo viaje,
/// se regresa el existente en lugar de responder "el carro ya tiene un viaje en curso".
/// </summary>
public record IniciarViajeCommand(
    Guid EmpresaId,
    Guid CarroId,
    Guid? SolicitudReservaId,
    Guid EmpleadoOperadorId,
    DateTime FechaInicio,
    decimal? UbicacionInicioLat,
    decimal? UbicacionInicioLng,
    decimal KilometrajeInicial,
    Guid? CentroCostoId = null,
    Guid? ViajeId = null) : IRequest<ViajeDto>, IRecursoPropio
{
    /// <summary>El operador registra SUS viajes; a nombre de otro, solo con viaje.corregir_cerrado.</summary>
    public bool EsPropio(IUsuarioActual usuario)
        => EmpleadoOperadorId == usuario.EmpleadoId;

    public string PrivilegioParaOtros => Permisos.ViajeCorregirCerrado;
}

public class IniciarViajeCommandHandler : IRequestHandler<IniciarViajeCommand, ViajeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IDateTimeProvider _dateTime;

    public IniciarViajeCommandHandler(
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

    public async Task<ViajeDto> Handle(IniciarViajeCommand request, CancellationToken cancellationToken)
    {
        // Reintento: el viaje ya se creó pero la respuesta no llegó al teléfono
        if (request.ViajeId is { } viajeId
            && await _unitOfWork.Viajes.GetByIdAsync(viajeId, cancellationToken) is { } existente)
        {
            ReintentoRegistro.ValidarViaje(existente, request.EmpresaId, request.CarroId, request.EmpleadoOperadorId);
            await AccesoViaje.ValidarOperadorAsync(_usuarioActual, existente, cancellationToken);

            var dtoExistente = await _mapper.Map<ViajeDto>(existente).ConUrlsAsync(_storage, cancellationToken);
            return await dtoExistente.ConPausasAsync(_unitOfWork, _mapper, _dateTime.Now, cancellationToken);
        }

        // El dominio valida los datos
        var viaje = Viaje.Iniciar(
            request.EmpresaId,
            request.CarroId,
            request.SolicitudReservaId,
            request.EmpleadoOperadorId,
            request.FechaInicio,
            request.UbicacionInicioLat,
            request.UbicacionInicioLng,
            request.KilometrajeInicial,
            request.CentroCostoId,
            request.ViajeId);

        await ViajeValidator.ValidarInicioAsync(_unitOfWork, viaje, esSincronizacion: false, cancellationToken);

        await _unitOfWork.Viajes.AddAsync(viaje, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _mapper.Map<ViajeDto>(viaje).ConUrlsAsync(_storage, cancellationToken);
    }
}
