using System;
using System.Collections.Generic;
using System.Text;
using APIGasoline.Application.Common.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace APIGasoline.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddAutoMapper(assembly);

        services.AddScoped<NotificadorPush>();
        services.AddScoped<ProcesadorOutbox>();

        return services;
    }
}