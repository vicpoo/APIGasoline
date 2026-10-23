using APIGasoline.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace APIGasoline.Application.Features.BaseDatos.Commands;

/// <summary>
/// Aplica al arrancar la API las migraciones pendientes (idempotente: si no hay pendientes no hace nada).
/// Se manda ANTES de <see cref="Auth.Commands.SembrarSeguridadCommand"/>, porque la siembra necesita las tablas.
/// Solo agrega lo que falta: nunca borra la base de datos ni sus datos.
/// </summary>
public record AplicarMigracionesCommand : IRequest;

public class AplicarMigracionesCommandHandler : IRequestHandler<AplicarMigracionesCommand>
{
    private readonly IMigradorBaseDatos _migrador;
    private readonly ILogger<AplicarMigracionesCommandHandler> _logger;

    public AplicarMigracionesCommandHandler(IMigradorBaseDatos migrador, ILogger<AplicarMigracionesCommandHandler> logger)
    {
        _migrador = migrador;
        _logger = logger;
    }

    public async Task Handle(AplicarMigracionesCommand request, CancellationToken cancellationToken)
    {
        var pendientes = await _migrador.ObtenerPendientesAsync(cancellationToken);

        if (pendientes.Count == 0)
        {
            _logger.LogInformation("Base de datos al día: no hay migraciones pendientes.");
            return;
        }

        _logger.LogInformation("Aplicando {Total} migración(es) pendiente(s): {Migraciones}",
            pendientes.Count, string.Join(", ", pendientes));

        await _migrador.AplicarPendientesAsync(cancellationToken);

        _logger.LogInformation("Migraciones aplicadas correctamente.");
    }
}
