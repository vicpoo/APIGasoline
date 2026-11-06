using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Sesiones;
using APIGasoline.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace APIGasoline.Infrastructure.Authentication;

/// <summary>
/// Corre en segundo plano y borra las sesiones que expiraron o se cerraron hace más de
/// Sesiones:LimpiezaRetencionDias días (ya no sirven para nada y la tabla crece con cada login).
/// </summary>
public class LimpiezaSesionesWorker : BackgroundService
{
    // Espera inicial para no competir con las migraciones y la siembra al arrancar
    private static readonly TimeSpan EsperaInicial = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SesionesSettings _settings;
    private readonly ILogger<LimpiezaSesionesWorker> _logger;

    public LimpiezaSesionesWorker(IServiceScopeFactory scopeFactory, SesionesSettings settings, ILogger<LimpiezaSesionesWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.LimpiezaHabilitada)
        {
            _logger.LogInformation("La limpieza de sesiones está deshabilitada (Sesiones:LimpiezaHabilitada = false).");
            return;
        }

        try
        {
            await Task.Delay(EsperaInicial, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var dateTime = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

                var limite = dateTime.Now.AddDays(-_settings.LimpiezaRetencionDias);
                var borradas = await unitOfWork.Sesiones.EliminarObsoletasAsync(limite, stoppingToken);

                if (borradas > 0)
                    _logger.LogInformation("Limpieza de sesiones: {Borradas} sesiones expiradas o cerradas eliminadas.", borradas);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // Por ejemplo, la BD no responde: se registra y se intenta en la siguiente vuelta
                _logger.LogError(ex, "Error al limpiar las sesiones viejas.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(_settings.LimpiezaIntervaloHoras), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
