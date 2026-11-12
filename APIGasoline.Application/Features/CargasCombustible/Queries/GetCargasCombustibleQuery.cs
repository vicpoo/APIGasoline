using APIGasoline.Domain.Common;
using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.CargasCombustible.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.CargasCombustible.Queries;

/// <summary>
/// Lista las cargas de la más reciente a la más antigua.
/// Filtros opcionales: empresa, viaje y rango de fechas.
/// </summary>
public sealed record GetCargasCombustibleQuery(
    Guid? EmpresaId = null,
    Guid? ViajeId = null,
    DateTime? Desde = null,
    DateTime? Hasta = null,
    int? Pagina = null,
    int? TamanoPagina = null) : IQuery<PaginaDe<CargaCombustibleDto>>;

public sealed class GetCargasCombustibleQueryHandler : IQueryHandler<GetCargasCombustibleQuery, PaginaDe<CargaCombustibleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;
    private readonly IUsuarioActual _usuarioActual;

    public GetCargasCombustibleQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
        _usuarioActual = usuarioActual;
    }

    public async Task<PaginaDe<CargaCombustibleDto>> Handle(GetCargasCombustibleQuery request, CancellationToken cancellationToken)
    {
        var empresaId = await _usuarioActual.ResolverEmpresaAsync(request.EmpresaId, cancellationToken);

        if (request.ViajeId is not null)
        {
            // Cargas de un viaje (detalle del viaje): solo si puede ver ese viaje
            var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId.Value, cancellationToken)
                ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

            await AccesoViaje.ValidarLecturaAsync(_usuarioActual, viaje, cancellationToken);
        }
        else if (!await _usuarioActual.TienePrivilegioAsync(Permisos.ViajeVerTodos, cancellationToken)
                 && !await _usuarioActual.TienePrivilegioAsync(Permisos.AuditoriaVer, cancellationToken))
        {
            // Todas las cargas de la empresa: solo supervisión o auditoría
            throw new ForbiddenException("Indica el viaje (?viajeId=) o necesitas el privilegio 'viaje.ver_todos'.");
        }

        var pagina = await _unitOfWork.CargasCombustible.GetPaginaAsync(
            new Paginacion(request.Pagina, request.TamanoPagina),
            empresaId, request.ViajeId, request.Desde, request.Hasta, cancellationToken);

        // Comprobantes firmados solo de la página que se devuelve
        return pagina.Con(await _mapper.Map<IReadOnlyList<CargaCombustibleDto>>(pagina.Elementos).ConUrlsAsync(_storage, cancellationToken));
    }
}
