using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.RolPrivilegios.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.RolPrivilegios.Queries;

public sealed record GetRolPrivilegioByIdQuery(Guid RolPrivilegioId) : IQuery<RolPrivilegioDto>;

public sealed class GetRolPrivilegioByIdQueryHandler : IQueryHandler<GetRolPrivilegioByIdQuery, RolPrivilegioDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetRolPrivilegioByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RolPrivilegioDto> Handle(GetRolPrivilegioByIdQuery request, CancellationToken cancellationToken)
    {
        var rolPrivilegio = await _unitOfWork.RolPrivilegios.GetByIdAsync(request.RolPrivilegioId, cancellationToken)
            ?? throw new NotFoundException($"No existe una asignación rol-privilegio con el id {request.RolPrivilegioId}.");

        return _mapper.Map<RolPrivilegioDto>(rolPrivilegio);
    }
}
