using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BS.WebAPI.ExceptionHandling;

/// <summary>
/// Convierte excepciones no controladas en respuestas <see cref="ProblemDetails"/> (RFC 7807).
/// </summary>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Excepción no controlada en {Path}", httpContext.Request.Path.Value);

        var (statusCode, title, detail) = MapException(exception, environment);

        var problemDetails = new ProblemDetails
        {
            Type = $"https://httpwg.org/specs/rfc9110.html#status.{statusCode}",
            Title = title,
            Detail = detail,
            Status = statusCode,
            Instance = httpContext.Request.Path.Value
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        };

        httpContext.Response.StatusCode = statusCode;
        await problemDetailsService.WriteAsync(context).ConfigureAwait(false);

        return true;
    }

    private static (int StatusCode, string Title, string? Detail) MapException(Exception exception, IHostEnvironment environment)
    {
        return exception switch
        {
            ArgumentNullException or ArgumentException or FormatException =>
                (StatusCodes.Status400BadRequest,
                    "Solicitud inválida.",
                    environment.IsDevelopment() ? exception.Message : exception.Message),

            InvalidOperationException =>
                (StatusCodes.Status400BadRequest,
                    "Operación no válida.",
                    environment.IsDevelopment() ? exception.Message : exception.Message),

            UnauthorizedAccessException =>
                (StatusCodes.Status403Forbidden,
                    "Acceso denegado.",
                    environment.IsDevelopment() ? exception.Message : null),

            NotImplementedException =>
                (StatusCodes.Status501NotImplemented,
                    "Funcionalidad no implementada.",
                    environment.IsDevelopment() ? exception.Message : null),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno del servidor.",
                environment.IsDevelopment()
                    ? exception.ToString()
                    : "Se produjo un error inesperado. Use el valor traceId de la respuesta si necesita contactar a soporte.")
        };
    }
}
