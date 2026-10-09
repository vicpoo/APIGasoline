using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Roles.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Roles.Queries;

public record GetRolByIdQuery(Guid RolId) : IRequest<RolDto>;

public class GetRolByIdQueryHandler : IRequestHandler<GetRolByIdQuery, RolDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetRolByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RolDto> Handle(GetRolByIdQuery request, CancellationToken cancellationToken)
    {
        var rol = await _unitOfWork.Roles.GetByIdAsync(request.RolId, cancellationToken)
            ?? throw new NotFoundException($"No existe un rol con el id {request.RolId}.");

        return _mapper.Map<RolDto>(rol);
    }
}
