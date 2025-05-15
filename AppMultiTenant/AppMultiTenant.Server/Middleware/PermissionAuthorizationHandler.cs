using Microsoft.AspNetCore.Authorization;

namespace AppMultiTenant.Server.Middleware
{
    /// <summary>
    /// Requisito de autorización para verificar que el usuario tiene un permiso específico
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        }
    }

    /// <summary>
    /// Manejador de autorización que verifica si el usuario tiene un permiso específico
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // Verificar si el usuario tiene claims de tipo "permission"
            var permissionClaims = context.User.Claims.Where(c => c.Type == "permission");

            // Si el usuario tiene el permiso requerido, se cumple el requisito
            if (permissionClaims.Any(c => c.Value == requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}