using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using APIGasoline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Regla de negocio no válida", ex.Message);
        }
        catch (BadRequestException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Solicitud no válida", ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, "No autorizado", ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status403Forbidden, "Acceso denegado", ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Recurso no encontrado", ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Conflicto", ex.Message, ConExtensiones(ex));
        }
        catch (DbUpdateConcurrencyException)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Conflicto",
                "Otro usuario modificó este registro al mismo tiempo. Actualiza la información e inténtalo de nuevo.");
        }
        catch (DbUpdateException ex) when (ViolacionUnicidad.EsDuplicado(ex, out var mensaje))
        {
            // Un dato único repetido (placas, correo...) que la validación del handler no vio:
            // con Row-Level Security pasa cuando el que ya lo tiene es de otra empresa.
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Conflicto", mensaje);
        }
        catch (ReintentarMasTardeException ex)
        {
            _logger.LogWarning(ex, "Operación que chocó con otra simultánea; se pidió reintentar.");
            await WriteReintentarAsync(context, ex.Message);
        }
        catch (Exception ex) when (Interbloqueo.Es(ex))
        {
            // N001: SQL Server eligió esta petición como víctima de un interbloqueo y la deshizo.
            // No hay nada mal en los datos: 503 + Retry-After para que el cliente la repita
            // (la app lo trata como error temporal; un 500 o un 409 no eran correctos).
            _logger.LogWarning(ex, "Interbloqueo de SQL Server (1205); se pidió reintentar.");
            await WriteReintentarAsync(context, "La operación chocó con otra al mismo tiempo. Vuelve a intentarlo en unos segundos.");
        }
        catch (AlmacenamientoException ex)
        {
            _logger.LogError(ex, "Falla del almacenamiento de archivos.");
            await WriteProblemAsync(context, StatusCodes.Status503ServiceUnavailable, "Almacenamiento no disponible", ex.Message);
        }
        catch (BadHttpRequestException ex) when (ex.StatusCode == StatusCodes.Status413PayloadTooLarge)
        {
            await WriteProblemAsync(context, StatusCodes.Status413PayloadTooLarge, "Archivo demasiado grande", "El cuerpo de la solicitud excede el tamaño permitido.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado.");
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Error interno", "Ocurrió un error inesperado.");
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        IDictionary<string, object?>? extensiones = null)
    {
        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        if (extensiones is not null)
        {
            foreach (var (clave, valor) in extensiones)
                problem.Extensions[clave] = valor;
        }

        await context.Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: "application/problem+json");
    }

    /// <summary>503 temporal con Retry-After: la misma petición funcionará si se repite.</summary>
    private static Task WriteReintentarAsync(HttpContext context, string detail)
    {
        context.Response.Headers.RetryAfter = SegundosParaReintentar.ToString();
        return WriteProblemAsync(context, StatusCodes.Status503ServiceUnavailable, "Intenta de nuevo", detail);
    }

    private const int SegundosParaReintentar = 2;

    /// <summary>"codigo" y los datos del conflicto, para que la app decida sin leer el mensaje.</summary>
    private static Dictionary<string, object?>? ConExtensiones(ConflictException ex)
    {
        if (ex.Codigo is null)
            return null;

        var extensiones = new Dictionary<string, object?> { ["codigo"] = ex.Codigo };

        if (ex.Datos is not null)
        {
            foreach (var (clave, valor) in ex.Datos)
                extensiones[clave] = valor;
        }

        return extensiones;
    }
}
