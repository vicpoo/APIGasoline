using APIGasoline.Application.Features.Privilegios.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Privilegios.Queries;

public record GetPrivilegiosQuery : IRequest<IReadOnlyList<PrivilegioDto>>;

public class GetPrivilegiosQueryHandler : IRequestHandler<GetPrivilegiosQuery, IReadOnlyList<PrivilegioDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPrivilegiosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PrivilegioDto>> Handle(GetPrivilegiosQuery request, CancellationToken cancellationToken)
    {
        var privilegios = await _unitOfWork.Privilegios.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<PrivilegioDto>>(privilegios);
    }
}
