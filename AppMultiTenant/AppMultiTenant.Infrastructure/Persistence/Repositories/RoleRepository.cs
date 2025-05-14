using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppMultiTenant.Application.Interfaces.Persistence;
using AppMultiTenant.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppMultiTenant.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementación del repositorio para la entidad ApplicationRole
    /// </summary>
    public class RoleRepository : RepositoryBase<ApplicationRole>, IRoleRepository
    {
        /// <summary>
        /// Constructor del repositorio de roles
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public RoleRepository(AppDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<ApplicationRole> GetByNameAsync(string roleName, Guid tenantId)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("El nombre del rol no puede estar vacío", nameof(roleName));

            if (tenantId == Guid.Empty)
                throw new ArgumentException("El tenantId no puede ser vacío", nameof(tenantId));

            return await _dbSet.FirstOrDefaultAsync(r => r.Name == roleName && r.TenantId == tenantId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationRole>> GetAllByTenantIdAsync(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El tenantId no puede ser vacío", nameof(tenantId));

            return await _dbSet.Where(r => r.TenantId == tenantId).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationRole>> GetRolesByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El userId no puede ser vacío", nameof(userId));

            // Consultamos los roles asignados al usuario mediante un join con la tabla UserRoles
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r)
                .Cast<ApplicationRole>()
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task AssignRoleToUserAsync(Guid userId, Guid roleId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El userId no puede ser vacío", nameof(userId));

            if (roleId == Guid.Empty)
                throw new ArgumentException("El roleId no puede ser vacío", nameof(roleId));

            // Verificamos si ya existe esta asignación
            bool exists = await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (!exists)
            {
                // Si no existe, creamos la asignación
                await _context.UserRoles.AddAsync(new IdentityUserRole<Guid>
                {
                    UserId = userId,
                    RoleId = roleId
                });
            }
        }

        /// <inheritdoc/>
        public async Task RemoveRoleFromUserAsync(Guid userId, Guid roleId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El userId no puede ser vacío", nameof(userId));

            if (roleId == Guid.Empty)
                throw new ArgumentException("El roleId no puede ser vacío", nameof(roleId));

            // Buscamos la asignación de rol al usuario
            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (userRole != null)
            {
                // Si existe, la eliminamos
                _context.UserRoles.Remove(userRole);
            }
        }
    }
} 