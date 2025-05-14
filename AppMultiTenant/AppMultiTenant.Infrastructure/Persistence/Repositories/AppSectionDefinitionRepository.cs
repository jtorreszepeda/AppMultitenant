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
    /// Implementación del repositorio para la entidad AppSectionDefinition
    /// </summary>
    public class AppSectionDefinitionRepository : RepositoryBase<AppSectionDefinition>, IAppSectionDefinitionRepository
    {
        /// <summary>
        /// Constructor del repositorio de definiciones de secciones
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public AppSectionDefinitionRepository(AppDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<AppSectionDefinition> GetByNameAsync(string name, Guid tenantId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre de la sección no puede estar vacío", nameof(name));

            if (tenantId == Guid.Empty)
                throw new ArgumentException("El tenantId no puede ser vacío", nameof(tenantId));

            // Normalizamos el nombre para la búsqueda
            string normalizedName = name.ToUpperInvariant();
            
            return await _dbSet.FirstOrDefaultAsync(s => 
                s.NormalizedName == normalizedName && s.TenantId == tenantId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AppSectionDefinition>> GetAllByTenantIdAsync(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El tenantId no puede ser vacío", nameof(tenantId));

            return await _dbSet.Where(s => s.TenantId == tenantId).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsByNameInTenantAsync(string name, Guid tenantId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre de la sección no puede estar vacío", nameof(name));

            if (tenantId == Guid.Empty)
                throw new ArgumentException("El tenantId no puede ser vacío", nameof(tenantId));

            // Normalizamos el nombre para la búsqueda
            string normalizedName = name.ToUpperInvariant();
            
            return await _dbSet.AnyAsync(s => 
                s.NormalizedName == normalizedName && s.TenantId == tenantId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AppSectionDefinition>> GetAccessibleSectionsForUserAsync(Guid userId, Guid tenantId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El userId no puede ser vacío", nameof(userId));

            if (tenantId == Guid.Empty)
                throw new ArgumentException("El tenantId no puede ser vacío", nameof(tenantId));

            // Verificamos primero si el usuario es administrador del tenant
            bool isAdmin = await _context.UserRoles
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { UserRole = ur, Role = r })
                .AnyAsync(joined => joined.UserRole.UserId == userId && 
                                   joined.Role.Name == "Admin" && 
                                   joined.Role.TenantId == tenantId);

            // Si es administrador, devolver todas las secciones del tenant
            if (isAdmin)
            {
                return await GetAllByTenantIdAsync(tenantId);
            }

            // Si no es administrador, verificar los permisos específicos
            // Obtener los permisos del usuario
            var userPermissions = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_context.RolePermissions,
                    ur => ur.RoleId,
                    rp => rp.Id,
                    (ur, rp) => rp)
                .Join(_context.Permissions,
                    rp => rp.PermissionId,
                    p => p.Id,
                    (rp, p) => p.Name)
                .Distinct()
                .ToListAsync();

            // Obtener todas las secciones del tenant
            var allSections = await GetAllByTenantIdAsync(tenantId);
            
            // Filtrar las secciones a las que el usuario tiene acceso (al menos permiso de lectura)
            return allSections.Where(section => 
                userPermissions.Any(permission => 
                    permission == $"CanReadDataInSection{section.NormalizedName}"));
        }
    }
} 