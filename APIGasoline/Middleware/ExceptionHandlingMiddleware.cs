using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
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
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Conflicto", ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Conflicto",
                "Otro usuario modificó este registro al mismo tiempo. Actualiza la información e inténtalo de nuevo.");
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

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            },
            options: null,
            contentType: "application/problem+json");
    }
}
