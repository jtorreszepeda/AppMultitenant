using System.Net;
using System.Text.Json;
using AppMultiTenant.Domain.Exceptions;

namespace AppMultiTenant.Server.Middleware
{
    /// <summary>
    /// Middleware que captura todas las excepciones no controladas en la aplicación
    /// y las transforma en respuestas HTTP estructuradas.
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
            catch (Exception ex)
            {
                // Log the exception with all relevant details
                _logger.LogError(ex, "An unhandled exception occurred during request processing. {RequestPath}", context.Request.Path);

                // Set up the HTTP response
                context.Response.ContentType = "application/json";

                // Determine the appropriate status code and message
                (HttpStatusCode statusCode, string message) = DetermineStatusCodeAndMessage(ex);
                context.Response.StatusCode = (int)statusCode;

                // Create an error response object
                var errorResponse = new
                {
                    StatusCode = (int)statusCode,
                    Message = message,
                    // Only include exception details in development environment
#if DEBUG
                    Detail = ex.ToString()
#endif
                };

                // Serialize the error response to JSON
                var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // Write the JSON response
                await context.Response.WriteAsync(jsonResponse);
            }
        }

        /// <summary>
        /// Determines the appropriate HTTP status code and message based on the exception type.
        /// </summary>
        private (HttpStatusCode StatusCode, string Message) DetermineStatusCodeAndMessage(Exception ex)
        {
            return ex switch
            {
                // Domain exceptions
                NotFoundException => (HttpStatusCode.NotFound, ex.Message),
                DomainValidationException => (HttpStatusCode.BadRequest, ex.Message),
                AccessDeniedException => (HttpStatusCode.Forbidden, ex.Message),
                TenantMismatchException => (HttpStatusCode.Forbidden, ex.Message),

                // Standard exceptions
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "No estás autorizado para realizar esta acción."),
                ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
                InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),

                // Default case for unexpected exceptions
                _ => (HttpStatusCode.InternalServerError, "Ha ocurrido un error inesperado. Por favor, inténtelo de nuevo más tarde.")
            };
        }
    }

    /// <summary>
    /// Extension methods to register the Global Exception Handling Middleware in the HTTP pipeline.
    /// </summary>
    public static class GlobalExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        }
    }
}