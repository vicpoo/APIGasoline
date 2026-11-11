using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Auditoria.Common;
using APIGasoline.Application.Features.Auditoria.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Auditoria.Queries;

public sealed record GetRevisionAuditoriaByIdQuery(Guid RevisionAuditoriaId) : IQuery<RevisionAuditoriaDto>;

public sealed class GetRevisionAuditoriaByIdQueryHandler : IQueryHandler<GetRevisionAuditoriaByIdQuery, RevisionAuditoriaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUsuarioActual _usuarioActual;

    public GetRevisionAuditoriaByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usuarioActual = usuarioActual;
    }

    public async Task<RevisionAuditoriaDto> Handle(GetRevisionAuditoriaByIdQuery request, CancellationToken cancellationToken)
    {
        var empresaPermitida = await PermisosAuditoria.ResolverEmpresaAsync(_usuarioActual, null, cancellationToken);

        var revision = await _unitOfWork.RevisionesAuditoria.GetByIdAsync(request.RevisionAuditoriaId, cancellationToken);

        if (revision is null || (empresaPermitida is not null && revision.EmpresaId != empresaPermitida))
            throw new NotFoundException($"No existe una revisión de auditoría con el id {request.RevisionAuditoriaId}.");

        return _mapper.Map<RevisionAuditoriaDto>(revision);
    }
}
