using Microsoft.AspNetCore.Authorization;

namespace AppMultiTenant.Server.Middleware
{
    /// <summary>
    /// Requisito de autorización para verificar que el usuario pertenece al inquilino actual
    /// </summary>
    public class TenantRequirement : IAuthorizationRequirement
    {
        public TenantRequirement() { }
    }

    /// <summary>
    /// Manejador de autorización que verifica que el usuario pertenece al inquilino actual
    /// </summary>
    public class TenantAuthorizationHandler : AuthorizationHandler<TenantRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TenantRequirement requirement)
        {
            // Obtener el TenantId almacenado en el contexto HTTP durante la resolución del inquilino
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return Task.CompletedTask; // No hay contexto HTTP, no podemos validar
            }

            // TenantId resuelto para la solicitud actual
            if (!httpContext.Items.TryGetValue("TenantId", out var resolvedTenantIdObj) ||
                resolvedTenantIdObj == null)
            {
                return Task.CompletedTask; // No hay TenantId resuelto, no podemos validar
            }

            // Verificar si el usuario tiene el claim "tenant_id"
            var userTenantIdClaim = context.User.FindFirst("tenant_id");
            if (userTenantIdClaim == null)
            {
                return Task.CompletedTask; // El usuario no tiene asignado un TenantId
            }

            // Si el TenantId del usuario coincide con el TenantId resuelto para la solicitud, se cumple el requisito
            if (Guid.TryParse(userTenantIdClaim.Value, out var userTenantId) &&
                Guid.TryParse(resolvedTenantIdObj.ToString(), out var resolvedTenantId) &&
                userTenantId == resolvedTenantId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}