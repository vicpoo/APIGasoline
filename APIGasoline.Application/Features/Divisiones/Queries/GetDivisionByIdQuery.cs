using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Divisiones.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Divisiones.Queries;

public record GetDivisionByIdQuery(int DivisionId) : IRequest<DivisionDto>;

public class GetDivisionByIdQueryHandler : IRequestHandler<GetDivisionByIdQuery, DivisionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDivisionByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<DivisionDto> Handle(GetDivisionByIdQuery request, CancellationToken cancellationToken)
    {
        var division = await _unitOfWork.Divisiones.GetByIdAsync(request.DivisionId, cancellationToken)
            ?? throw new NotFoundException($"No existe una división con el id {request.DivisionId}.");

        return _mapper.Map<DivisionDto>(division);
    }
}
