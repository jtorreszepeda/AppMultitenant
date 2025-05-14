using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppMultiTenant.Application.Interfaces.Persistence;
using AppMultiTenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppMultiTenant.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementación del repositorio para la entidad ApplicationUser
    /// </summary>
    public class UserRepository : RepositoryBase<ApplicationUser>, IUserRepository
    {
        /// <summary>
        /// Constructor del repositorio de usuarios
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<ApplicationUser> GetByEmailAsync(string email, Guid tenantId)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El email no puede estar vacío", nameof(email));

            if (tenantId == Guid.Empty)
                throw new ArgumentException("El tenantId no puede ser vacío", nameof(tenantId));

            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && u.TenantId == tenantId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationUser>> GetAllByTenantIdAsync(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El tenantId no puede ser vacío", nameof(tenantId));

            return await _dbSet.Where(u => u.TenantId == tenantId).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationUser>> GetAllActiveByTenantIdAsync(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El tenantId no puede ser vacío", nameof(tenantId));

            return await _dbSet.Where(u => u.TenantId == tenantId && !u.LockoutEnabled).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAdminInTenantAsync(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El tenantId no puede ser vacío", nameof(tenantId));

            // Consultamos si hay usuarios en el tenant que tengan el rol de administrador
            // Esto requiere hacer un join con la tabla UserRoles y Roles
            return await _context.UserRoles
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { UserRole = ur, Role = r })
                .Join(_context.Users,
                    joined => joined.UserRole.UserId,
                    u => u.Id,
                    (joined, u) => new { User = u, Role = joined.Role })
                .AnyAsync(joined => joined.User.TenantId == tenantId && 
                                    joined.Role.Name == "Admin" && 
                                    joined.Role.TenantId == tenantId);
        }
    }
} 