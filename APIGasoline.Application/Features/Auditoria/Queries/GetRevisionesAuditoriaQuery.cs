using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Auditoria.Common;
using APIGasoline.Application.Features.Auditoria.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Auditoria.Queries;

/// <summary>Revisiones de la más reciente a la más antigua. ?abiertas=true para las que siguen sin cerrar.</summary>
public record GetRevisionesAuditoriaQuery(
    Guid? EmpresaId,
    Guid? ViajeId,
    Guid? CarroId,
    EstadoRevision? Estado,
    bool? Abiertas) : IRequest<IReadOnlyList<RevisionAuditoriaDto>>;

public class GetRevisionesAuditoriaQueryHandler : IRequestHandler<GetRevisionesAuditoriaQuery, IReadOnlyList<RevisionAuditoriaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUsuarioActual _usuarioActual;

    public GetRevisionesAuditoriaQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usuarioActual = usuarioActual;
    }

    public async Task<IReadOnlyList<RevisionAuditoriaDto>> Handle(GetRevisionesAuditoriaQuery request, CancellationToken cancellationToken)
    {
        var empresaId = PermisosAuditoria.ResolverEmpresa(_usuarioActual, request.EmpresaId);

        var revisiones = await _unitOfWork.RevisionesAuditoria.GetAllAsync(
            empresaId,
            request.ViajeId,
            request.CarroId,
            request.Estado,
            request.Abiertas,
            cancellationToken);

        return _mapper.Map<IReadOnlyList<RevisionAuditoriaDto>>(revisiones);
    }
}
