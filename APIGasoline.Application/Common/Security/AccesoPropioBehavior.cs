using APIGasoline.Application.Common.Interfaces;
using MediatR;

namespace APIGasoline.Application.Common.Security;

/// <summary>
/// Pipeline de MediatR: para los requests que implementan <see cref="IRecursoPropio"/> valida
/// "lo tuyo, o lo de otros con privilegio" antes de llegar al handler. Así la regla vive en
/// Application y los controladores solo hacen <c>_sender.Send(...)</c>.
/// </summary>
public sealed class AccesoPropioBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUsuarioActual _usuarioActual;

    public AccesoPropioBehavior(IUsuarioActual usuarioActual)
    {
        _usuarioActual = usuarioActual;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IRecursoPropio recurso)
        {
            await _usuarioActual.ValidarPropioOPrivilegioAsync(
                recurso.EsPropio(_usuarioActual),
                recurso.PrivilegioParaOtros,
                cancellationToken);
        }

        return await next();
    }
}
