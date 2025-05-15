using AppMultiTenant.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AppMultiTenant.Server.Middleware
{
    /// <summary>
    /// Filtro de acción para validar entidades de dominio recibidas en las solicitudes
    /// </summary>
    public class ModelValidationFilter : IAsyncActionFilter
    {
        private readonly IValidationService _validationService;

        /// <summary>
        /// Constructor del filtro de validación
        /// </summary>
        /// <param name="validationService">Servicio de validación</param>
        public ModelValidationFilter(IValidationService validationService)
        {
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        }

        /// <summary>
        /// Ejecuta el filtro de acción
        /// </summary>
        /// <param name="context">Contexto de la acción</param>
        /// <param name="next">Delegado para continuar la ejecución</param>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Primero verificamos si el ModelState es válido (validaciones de DataAnnotations)
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
                return;
            }

            // Luego validamos las entidades de dominio con FluentValidation
            foreach (var argument in context.ActionArguments)
            {
                var value = argument.Value;
                if (value == null) continue;

                // Obtenemos el tipo del argumento
                var valueType = value.GetType();

                // Validamos usando el servicio de validación
                try
                {
                    var validationResult = await _validationService.ValidateAsync(value);
                    if (!validationResult.IsValid)
                    {
                        // Agregamos los errores al ModelState
                        foreach (var error in validationResult.Errors)
                        {
                            context.ModelState.AddModelError(
                                string.IsNullOrEmpty(error.PropertyName) ? argument.Key : error.PropertyName,
                                error.ErrorMessage);
                        }

                        context.Result = new BadRequestObjectResult(context.ModelState);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    // Si ocurre un error durante la validación, lo registramos pero no bloqueamos la solicitud
                    // Para mantener la compatibilidad con controladores existentes
                    context.HttpContext.RequestServices
                        .GetService<Microsoft.Extensions.Logging.ILogger<ModelValidationFilter>>()?
                        .LogWarning(ex, "Error durante la validación del modelo {ModelType}", valueType.Name);
                }
            }

            // Continuar con la ejecución
            await next();
        }
    }
}