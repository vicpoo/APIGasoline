using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Carros.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Carros.Commands;

public record UpdateCarroCommand(
    int CarroId,
    int EmpresaId,
    string Marca,
    string Modelo,
    int? Anio,
    string? Color,
    string Placas,
    string? FotoVehiculo,
    decimal? CapacidadTanque,
    decimal KilometrajeActual) : IRequest<CarroDto>;

public class UpdateCarroCommandHandler : IRequestHandler<UpdateCarroCommand, CarroDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCarroCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CarroDto> Handle(UpdateCarroCommand request, CancellationToken cancellationToken)
    {
        var carro = await _unitOfWork.Carros.GetByIdAsync(request.CarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un carro con el id {request.CarroId}.");

        // El dominio valida y normaliza los datos
        carro.CambiarEmpresa(request.EmpresaId);
        carro.CambiarDatos(request.Marca, request.Modelo, request.Anio, request.Color);
        carro.CambiarPlacas(request.Placas);
        carro.CambiarFotoVehiculo(request.FotoVehiculo);
        carro.CambiarCapacidadTanque(request.CapacidadTanque);
        carro.ActualizarKilometraje(request.KilometrajeActual);
        // El estado NO se cambia aquí: usa /api/cambios-estatus-carro (queda en la bitácora)

        if (!await _unitOfWork.Empresas.ExistsAsync(carro.EmpresaId, cancellationToken))
            throw new NotFoundException($"No existe una empresa con el id {carro.EmpresaId}.");

        if (await _unitOfWork.Carros.PlacasExistsAsync(carro.Placas, carro.CarroId, cancellationToken))
            throw new ConflictException($"Ya existe otro carro con las placas '{carro.Placas}'.");

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CarroDto>(carro);
    }
}
