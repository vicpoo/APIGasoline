using APIGasoline.Application.Common.Interfaces;

namespace APIGasoline.Middleware;

/// <summary>
/// Va justo después de la autenticación: con el usuario del token ya leído, decide de qué empresa son
/// los datos de esta petición (<see cref="IContextoEmpresa"/>). Desde aquí, cada consulta de EF trae
/// solo lo de esa empresa y el guardián de SaveChanges no deja escribir en otra.
/// </summary>
public sealed class ContextoEmpresaMiddleware
{
    private readonly RequestDelegate _next;

    public ContextoEmpresaMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IContextoEmpresa contextoEmpresa)
    {
        await contextoEmpresa.InicializarAsync(context.RequestAborted);
        await _next(context);
    }
}
