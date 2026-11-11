using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Privilegios.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Privilegios.Queries;

public sealed record GetPrivilegioByIdQuery(Guid PrivilegioId) : IQuery<PrivilegioDto>;

public sealed class GetPrivilegioByIdQueryHandler : IQueryHandler<GetPrivilegioByIdQuery, PrivilegioDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPrivilegioByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PrivilegioDto> Handle(GetPrivilegioByIdQuery request, CancellationToken cancellationToken)
    {
        var privilegio = await _unitOfWork.Privilegios.GetByIdAsync(request.PrivilegioId, cancellationToken)
            ?? throw new NotFoundException($"No existe un privilegio con el id {request.PrivilegioId}.");

        return _mapper.Map<PrivilegioDto>(privilegio);
    }
}
