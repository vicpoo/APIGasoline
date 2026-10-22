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
    Guid? CentroCostoId = null) : IRequest<ViajeDto>, IRecursoPropio
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

    public IniciarViajeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<ViajeDto> Handle(IniciarViajeCommand request, CancellationToken cancellationToken)
    {
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
            request.CentroCostoId);

        await ViajeValidator.ValidarInicioAsync(_unitOfWork, viaje, esSincronizacion: false, cancellationToken);

        await _unitOfWork.Viajes.AddAsync(viaje, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _mapper.Map<ViajeDto>(viaje).ConUrlsAsync(_storage, cancellationToken);
    }
}
