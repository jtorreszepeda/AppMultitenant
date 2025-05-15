using Microsoft.AspNetCore.Mvc;

namespace AppMultiTenant.Server.Middleware
{
    /// <summary>
    /// Extensiones para registrar servicios de validación
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Configura los servicios de validación para la aplicación
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <returns>Colección de servicios con validación configurada</returns>
        public static IServiceCollection AddModelValidation(this IServiceCollection services)
        {
            // Configurar MVC con el filtro de validación de modelos
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add<ModelValidationFilter>();
            });

            return services;
        }
    }
}