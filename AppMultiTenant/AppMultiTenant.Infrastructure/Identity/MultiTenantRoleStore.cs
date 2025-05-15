using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AppMultiTenant.Domain.Entities;
using AppMultiTenant.Application.Interfaces.Services;
using AppMultiTenant.Infrastructure.Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppMultiTenant.Infrastructure.Identity
{
    /// <summary>
    /// Implementación personalizada de RoleStore para tener en cuenta el TenantId
    /// en las operaciones de Identity
    /// </summary>
    public class MultiTenantRoleStore : RoleStore<ApplicationRole, AppDbContext, Guid>
    {
        private readonly Guid? _currentTenantId;

        public MultiTenantRoleStore(
            AppDbContext context,
            ITenantResolverService tenantResolverService,
            IdentityErrorDescriber errorDescriber = null)
            : base(context, errorDescriber)
        {
            _currentTenantId = tenantResolverService.GetCurrentTenantId();
        }

        /// <summary>
        /// Sobrescribe el método de creación para asegurar que el rol se asocie con
        /// el inquilino correcto
        /// </summary>
        public override async Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken = default)
        {
            // Si el inquilino ya está establecido, verificamos que coincida con el contexto actual
            if (role.TenantId != default && _currentTenantId.HasValue && role.TenantId != _currentTenantId)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidTenantId",
                    Description = "No se puede crear un rol para un inquilino diferente al contexto actual."
                });
            }

            // Establecemos el TenantId si el rol es nuevo y no tiene uno asignado
            if (role.TenantId == default && _currentTenantId.HasValue)
            {
                role.TenantId = _currentTenantId.Value;
            }

            return await base.CreateAsync(role, cancellationToken);
        }

        /// <summary>
        /// Sobrescribe el método de búsqueda para tener en cuenta el inquilino actual
        /// </summary>
        public override Task<ApplicationRole> FindByIdAsync(string roleId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var id = ConvertIdFromString(roleId);

            // Si no hay un inquilino en el contexto, permitimos la operación (modo SuperAdmin)
            if (!_currentTenantId.HasValue)
            {
                return Roles.FirstOrDefaultAsync(r => r.Id.Equals(id), cancellationToken);
            }

            // En modo tenant, filtramos por TenantId
            return Roles.FirstOrDefaultAsync(r => 
                r.Id.Equals(id) && r.TenantId == _currentTenantId, 
                cancellationToken);
        }

        /// <summary>
        /// Sobrescribe el método de búsqueda por nombre para tener en cuenta el inquilino actual
        /// </summary>
        public override Task<ApplicationRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            // Si no hay un inquilino en el contexto, permitimos la operación (modo SuperAdmin)
            if (!_currentTenantId.HasValue)
            {
                return Roles.FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName, cancellationToken);
            }

            // En modo tenant, filtramos por TenantId
            return Roles.FirstOrDefaultAsync(r => 
                r.NormalizedName == normalizedRoleName && r.TenantId == _currentTenantId, 
                cancellationToken);
        }
    }
} 