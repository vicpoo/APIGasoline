using APIGasoline.Application.Common.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace APIGasoline.Infrastructure.Outbox;

/// <summary>
/// Corre en segundo plano mientras la API está encendida. Cada vuelta crea un scope nuevo
/// (un DbContext limpio) y le pide al ProcesadorOutbox que envíe un lote.
/// </summary>
public class OutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OutboxSettings _settings;
    private readonly ILogger<OutboxWorker> _logger;

    public OutboxWorker(IServiceScopeFactory scopeFactory, OutboxSettings settings, ILogger<OutboxWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Habilitado)
        {
            _logger.LogInformation("El worker del outbox está deshabilitado (Outbox:Habilitado = false).");
            return;
        }

        _logger.LogInformation("Worker del outbox iniciado: cada {Intervalo} s, lotes de {Lote}.", _settings.IntervaloSegundos, _settings.TamanoLote);

        while (!stoppingToken.IsCancellationRequested)
        {
            var procesados = 0;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var procesador = scope.ServiceProvider.GetRequiredService<ProcesadorOutbox>();

                procesados = await procesador.ProcesarLoteAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // Por ejemplo, la BD no responde: se registra y se intenta en la siguiente vuelta
                _logger.LogError(ex, "Error al procesar el outbox.");
            }

            // Si el lote venía lleno probablemente hay más: se sigue sin esperar
            if (procesados >= _settings.TamanoLote)
                continue;

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_settings.IntervaloSegundos), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
