using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Messaging;
using APIGasoline.Application.Features.Corporativos.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;

namespace APIGasoline.Application.Features.Corporativos.Commands;

/// <summary>El corporativo pertenece a una empresa (Fase 8).</summary>
public sealed record CreateCorporativoCommand(Guid EmpresaId, string Nombre, bool Activo) : ICommand<CorporativoDto>;

public sealed class CreateCorporativoCommandHandler : ICommandHandler<CreateCorporativoCommand, CorporativoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCorporativoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CorporativoDto> Handle(CreateCorporativoCommand request, CancellationToken cancellationToken)
    {
        // El dominio valida y normaliza los datos
        var corporativo = Corporativo.Crear(request.EmpresaId, request.Nombre, request.Activo);

        // Las empresas eliminadas (soft delete) y las de otra empresa no cuentan como existentes
        if (!await _unitOfWork.Empresas.ExistsAsync(corporativo.EmpresaId, cancellationToken))
            throw new NotFoundException($"No existe una empresa con el id {corporativo.EmpresaId}.");

        if (await _unitOfWork.Corporativos.NombreExistsAsync(corporativo.EmpresaId, corporativo.Nombre, null, cancellationToken))
            throw new ConflictException($"La empresa {corporativo.EmpresaId} ya tiene un corporativo llamado '{corporativo.Nombre}'.");

        await _unitOfWork.Corporativos.AddAsync(corporativo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CorporativoDto>(corporativo);
    }
}
