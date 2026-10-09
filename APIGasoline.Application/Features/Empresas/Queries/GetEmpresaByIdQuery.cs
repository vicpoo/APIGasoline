using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Empresas.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Empresas.Queries;

public record GetEmpresaByIdQuery(Guid EmpresaId) : IRequest<EmpresaDto>;

public class GetEmpresaByIdQueryHandler : IRequestHandler<GetEmpresaByIdQuery, EmpresaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetEmpresaByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<EmpresaDto> Handle(GetEmpresaByIdQuery request, CancellationToken cancellationToken)
    {
        var empresa = await _unitOfWork.Empresas.GetByIdAsync(request.EmpresaId, cancellationToken)
            ?? throw new NotFoundException($"No existe una empresa con el id {request.EmpresaId}.");

        return _mapper.Map<EmpresaDto>(empresa);
    }
}
