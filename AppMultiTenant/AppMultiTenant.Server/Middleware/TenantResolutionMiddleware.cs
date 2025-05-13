using AppMultiTenant.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;

namespace AppMultiTenant.Server.Middleware
{
    /// <summary>
    /// Middleware que resuelve el inquilino actual para cada solicitud HTTP
    /// y lo establece en el contexto para que otros componentes puedan acceder a él.
    /// </summary>
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantResolutionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITenantResolverService tenantResolverService)
        {
            // Resuelve el inquilino para la solicitud actual
            var tenantId = tenantResolverService.GetCurrentTenantId();
            var tenantIdentifier = tenantResolverService.GetCurrentTenantIdentifier();

            // Guarda la información del inquilino en el diccionario Items del HttpContext
            // para que otros componentes en el pipeline puedan acceder a ella
            if (tenantId.HasValue)
            {
                context.Items["TenantId"] = tenantId.Value;
            }
            
            if (!string.IsNullOrEmpty(tenantIdentifier))
            {
                context.Items["TenantIdentifier"] = tenantIdentifier;
            }

            // Continúa procesando la solicitud
            await _next(context);
        }
    }

    /// <summary>
    /// Extensiones para registrar fácilmente el middleware en la pipeline HTTP
    /// </summary>
    public static class TenantResolutionMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantResolution(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantResolutionMiddleware>();
        }
    }
} 