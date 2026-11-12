using APIGasoline.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace APIGasoline.Extensions;

/// <summary>
/// Listados paginados (F011): el cuerpo sigue siendo una lista (no se rompe el formato que ya usan
/// las apps) y los datos de la página van en headers:
/// X-Total-Count (cuántos hay con esos filtros), X-Pagina y X-Tamano-Pagina.
/// Se piden con ?pagina= (desde 1) y ?tamanoPagina= (por defecto 50, máximo 200).
/// </summary>
public static class PaginacionExtensions
{
    public const string HeaderTotal = "X-Total-Count";
    public const string HeaderPagina = "X-Pagina";
    public const string HeaderTamanoPagina = "X-Tamano-Pagina";

    public static ActionResult<IReadOnlyList<T>> OkPaginado<T>(this ControllerBase controller, PaginaDe<T> pagina)
    {
        var headers = controller.Response.Headers;
        headers[HeaderTotal] = pagina.Total.ToString();
        headers[HeaderPagina] = pagina.Paginacion.Pagina.ToString();
        headers[HeaderTamanoPagina] = pagina.Paginacion.TamanoPagina.ToString();

        return controller.Ok(pagina.Elementos);
    }
}
