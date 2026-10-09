using APIGasoline.Application.Features.Empresas.Common;
using APIGasoline.Application.Features.Empresas.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Empresas.Commands;

public record CreateEmpresaCommand(string Nombre, Guid? CorporativoId, Guid? DivisionId) : IRequest<EmpresaDto>;

public class CreateEmpresaCommandHandler : IRequestHandler<CreateEmpresaCommand, EmpresaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateEmpresaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<EmpresaDto> Handle(CreateEmpresaCommand request, CancellationToken cancellationToken)
    {
        // El dominio valida y normaliza los datos
        var empresa = Empresa.Crear(request.Nombre, request.CorporativoId, request.DivisionId);

        await EmpresaOrganizacionValidator.ValidarAsync(_unitOfWork, empresa.CorporativoId, empresa.DivisionId, cancellationToken);

        await _unitOfWork.Empresas.AddAsync(empresa, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // EmpresaId y FechaRegistro ya vienen asignados por la base de datos
        return _mapper.Map<EmpresaDto>(empresa);
    }
}
