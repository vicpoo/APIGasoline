using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Domain.Rutas;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.CargasCombustible.Commands;

/// <summary>
/// Completa la ubicación de las cargas del viaje que llegaron sin ella, con el punto GPS de la ruta
/// más cercano a la hora de la carga (ver <see cref="UbicacionCargaEstimador"/>).
/// Las cargas que ya tienen ubicación no se tocan. Si todavía no hay puntos cerca (por ejemplo,
/// llegaron antes que la ruta por estar sin internet), se vuelve a intentar cuando lleguen.
/// </summary>
/// <returns>Cuántas cargas se ubicaron.</returns>
public sealed record CompletarUbicacionCargasCommand(Guid ViajeId) : ICommand<int>;

public sealed class CompletarUbicacionCargasCommandHandler : ICommandHandler<CompletarUbicacionCargasCommand, int>
{
    public const string MotivoAuditoria = "Ubicación estimada con el punto GPS de la ruta más cercano a la hora de la carga.";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IContextoAuditoria _auditoria;

    public CompletarUbicacionCargasCommandHandler(IUnitOfWork unitOfWork, IContextoAuditoria auditoria)
    {
        _unitOfWork = unitOfWork;
        _auditoria = auditoria;
    }

    public async Task<int> Handle(CompletarUbicacionCargasCommand request, CancellationToken cancellationToken)
    {
        var cargas = await _unitOfWork.CargasCombustible.GetSinUbicacionByViajeAsync(request.ViajeId, cancellationToken);
        if (cargas.Count == 0)
            return 0;

        var puntos = await _unitOfWork.PuntosRuta.GetByViajeAsync(request.ViajeId, cancellationToken);
        if (puntos.Count == 0)
            return 0;

        var completadas = 0;
        foreach (var carga in cargas)
        {
            if (UbicacionCargaEstimador.Estimar(puntos, carga.FechaCarga) is not { } ubicacion)
                continue;

            if (carga.CompletarUbicacion(ubicacion.Latitud, ubicacion.Longitud))
            {
                // Queda en la auditoría que la ubicación la puso el servidor, no el teléfono
                _auditoria.Registrar(carga.CargaCombustibleId, AccionAuditoria.Actualizar, MotivoAuditoria);
                completadas++;
            }
        }

        if (completadas > 0)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        return completadas;
    }
}
