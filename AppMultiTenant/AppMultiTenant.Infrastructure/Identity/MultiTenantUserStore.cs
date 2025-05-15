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
using System.Collections.Generic;

namespace AppMultiTenant.Infrastructure.Identity
{
    /// <summary>
    /// Implementación personalizada de UserStore para tener en cuenta el TenantId
    /// en las operaciones de Identity
    /// </summary>
    public class MultiTenantUserStore : UserStore<ApplicationUser, ApplicationRole, AppDbContext, Guid>
    {
        private readonly Guid? _currentTenantId;

        public MultiTenantUserStore(
            AppDbContext context,
            ITenantResolverService tenantResolverService,
            IdentityErrorDescriber errorDescriber = null)
            : base(context, errorDescriber)
        {
            _currentTenantId = tenantResolverService.GetCurrentTenantId();
        }

        /// <summary>
        /// Sobrescribe el método de creación para asegurar que el usuario se asocie con
        /// el inquilino correcto
        /// </summary>
        public override async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            // Si el inquilino ya está establecido, verificamos que coincida con el contexto actual
            if (user.TenantId != default && _currentTenantId.HasValue && user.TenantId != _currentTenantId)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "InvalidTenantId",
                    Description = "No se puede crear un usuario para un inquilino diferente al contexto actual."
                });
            }

            // Establecemos el TenantId si el usuario es nuevo y no tiene uno asignado
            if (user.TenantId == default && _currentTenantId.HasValue)
            {
                user.TenantId = _currentTenantId.Value;
            }

            return await base.CreateAsync(user, cancellationToken);
        }

        /// <summary>
        /// Sobrescribe el método de búsqueda para tener en cuenta el inquilino actual
        /// </summary>
        public override Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var id = ConvertIdFromString(userId);

            // Si no hay un inquilino en el contexto, permitimos la operación (modo SuperAdmin)
            if (!_currentTenantId.HasValue)
            {
                return Users.FirstOrDefaultAsync(u => u.Id.Equals(id), cancellationToken);
            }

            // En modo tenant, filtramos por TenantId
            return Users.FirstOrDefaultAsync(u => 
                u.Id.Equals(id) && u.TenantId == _currentTenantId, 
                cancellationToken);
        }

        /// <summary>
        /// Sobrescribe el método de búsqueda por nombre para tener en cuenta el inquilino actual
        /// </summary>
        public override Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            // Si no hay un inquilino en el contexto, permitimos la operación (modo SuperAdmin)
            if (!_currentTenantId.HasValue)
            {
                return Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);
            }

            // En modo tenant, filtramos por TenantId
            return Users.FirstOrDefaultAsync(u => 
                u.NormalizedUserName == normalizedUserName && u.TenantId == _currentTenantId, 
                cancellationToken);
        }

        /// <summary>
        /// Sobrescribe el método de búsqueda por email para tener en cuenta el inquilino actual
        /// </summary>
        public override Task<ApplicationUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            // Si no hay un inquilino en el contexto, permitimos la operación (modo SuperAdmin)
            if (!_currentTenantId.HasValue)
            {
                return Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
            }

            // En modo tenant, filtramos por TenantId
            return Users.FirstOrDefaultAsync(u => 
                u.NormalizedEmail == normalizedEmail && u.TenantId == _currentTenantId, 
                cancellationToken);
        }

        /// <summary>
        /// Sobrescribe el método para obtener usuarios en un rol y filtra por TenantId
        /// </summary>
        public override async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var query = from userRole in Context.UserRoles
                       join user in Users on userRole.UserId equals user.Id
                       join role in Context.Roles on userRole.RoleId equals role.Id
                       where role.NormalizedName == normalizedRoleName
                       select user;

            // Si hay un inquilino en el contexto, filtramos usuarios por TenantId
            if (_currentTenantId.HasValue)
            {
                query = query.Where(user => user.TenantId == _currentTenantId);
            }

            return await query.ToListAsync(cancellationToken);
        }
    }
} 