using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Empresas.Common;
using APIGasoline.Application.Features.Empresas.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Empresas.Commands;

public record UpdateEmpresaCommand(Guid EmpresaId, string Nombre, Guid? CorporativoId, Guid? DivisionId) : IRequest<EmpresaDto>;

public class UpdateEmpresaCommandHandler : IRequestHandler<UpdateEmpresaCommand, EmpresaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateEmpresaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<EmpresaDto> Handle(UpdateEmpresaCommand request, CancellationToken cancellationToken)
    {
        var empresa = await _unitOfWork.Empresas.GetByIdAsync(request.EmpresaId, cancellationToken)
            ?? throw new NotFoundException($"No existe una empresa con el id {request.EmpresaId}.");

        // El dominio valida y normaliza los datos
        empresa.CambiarNombre(request.Nombre);
        empresa.AsignarOrganizacion(request.CorporativoId, request.DivisionId);

        await EmpresaOrganizacionValidator.ValidarAsync(_unitOfWork, empresa.CorporativoId, empresa.DivisionId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<EmpresaDto>(empresa);
    }
}
