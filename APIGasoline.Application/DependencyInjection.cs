using System;
using System.Collections.Generic;
using System.Text;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Outbox;
using APIGasoline.Application.Common.Security;
using Microsoft.Extensions.DependencyInjection;

namespace APIGasoline.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // "Lo tuyo, o lo de otros con privilegio" (requests que implementan IRecursoPropio)
            cfg.AddOpenBehavior(typeof(AccesoPropioBehavior<,>));
        });
        // AutoMapper 15+: la licencia se toma de la variable de entorno AUTOMAPPER_LICENSE_KEY
        // (sin licencia solo escribe un aviso en el log y sigue funcionando)
        services.AddAutoMapper(_ => { }, assembly);

        // Aislamiento entre empresas: de qué empresa son los datos de la petición (filtro de EF y guardián)
        services.AddScoped<IContextoEmpresa, ContextoEmpresa>();

        services.AddScoped<NotificadorPush>();
        services.AddScoped<ProcesadorOutbox>();

        return services;
    }
}