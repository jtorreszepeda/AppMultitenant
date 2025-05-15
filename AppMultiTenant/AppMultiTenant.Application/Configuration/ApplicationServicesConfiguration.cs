using Microsoft.Extensions.DependencyInjection;
using AppMultiTenant.Application.Interfaces.Services;
using AppMultiTenant.Application.Services;

namespace AppMultiTenant.Application.Configuration
{
    /// <summary>
    /// Contiene métodos de extensión para configurar servicios de la capa de aplicación
    /// </summary>
    public static class ApplicationServicesConfiguration
    {
        /// <summary>
        /// Agrega los servicios de la capa de aplicación al contenedor de DI
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <returns>Colección de servicios actualizada</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Registrar servicios de aplicación
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ISystemAdminTenantService, SystemAdminTenantService>();
            services.AddScoped<ITenantUserService, TenantUserService>();
            services.AddScoped<ITenantRoleService, TenantRoleService>();
            services.AddScoped<ITenantSectionDefinitionService, TenantSectionDefinitionService>();
            services.AddScoped<IValidationService, ValidationService>();
            
            return services;
        }
    }
} 