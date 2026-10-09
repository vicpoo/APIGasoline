using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.CentrosCosto.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.CentrosCosto.Commands;

public record UpdateCentroCostoCommand(Guid CentroCostoId, Guid EmpresaId, string Nombre, string Clave, bool Activo) : IRequest<CentroCostoDto>;

public class UpdateCentroCostoCommandHandler : IRequestHandler<UpdateCentroCostoCommand, CentroCostoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCentroCostoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CentroCostoDto> Handle(UpdateCentroCostoCommand request, CancellationToken cancellationToken)
    {
        var centroCosto = await _unitOfWork.CentrosCosto.GetByIdAsync(request.CentroCostoId, cancellationToken)
            ?? throw new NotFoundException($"No existe un centro de costo con el id {request.CentroCostoId}.");

        // El dominio valida y normaliza los datos
        centroCosto.CambiarEmpresa(request.EmpresaId);
        centroCosto.CambiarNombre(request.Nombre);
        centroCosto.CambiarClave(request.Clave);

        if (request.Activo)
            centroCosto.Activar();
        else
            centroCosto.Desactivar();

        if (!await _unitOfWork.Empresas.ExistsAsync(centroCosto.EmpresaId, cancellationToken))
            throw new NotFoundException($"No existe una empresa con el id {centroCosto.EmpresaId}.");

        if (await _unitOfWork.CentrosCosto.ClaveExistsAsync(centroCosto.EmpresaId, centroCosto.Clave, centroCosto.CentroCostoId, cancellationToken))
            throw new ConflictException($"La empresa {centroCosto.EmpresaId} ya tiene otro centro de costo con la clave '{centroCosto.Clave}'.");

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CentroCostoDto>(centroCosto);
    }
}
