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
    /// Implementación del repositorio para la entidad Permission
    /// </summary>
    public class PermissionRepository : RepositoryBase<Permission>, IPermissionRepository
    {
        /// <summary>
        /// Constructor del repositorio de permisos
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public PermissionRepository(AppDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<Permission> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre del permiso no puede estar vacío", nameof(name));

            return await _dbSet.FirstOrDefaultAsync(p => p.Name == name);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId)
        {
            if (roleId == Guid.Empty)
                throw new ArgumentException("El roleId no puede ser vacío", nameof(roleId));

            // Consultamos los permisos asignados al rol mediante un join con la tabla RolePermissions
            return await _context.RolePermissions
                .Where(rp => rp.Id == roleId)
                .Join(_context.Permissions,
                    rp => rp.PermissionId,
                    p => p.Id,
                    (rp, p) => p)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El userId no puede ser vacío", nameof(userId));

            // Consultamos los permisos a través de los roles del usuario
            // Esto requiere un join entre UserRoles, RolePermissions y Permissions
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_context.RolePermissions,
                    ur => ur.RoleId,
                    rp => rp.Id,
                    (ur, rp) => rp)
                .Join(_context.Permissions,
                    rp => rp.PermissionId,
                    p => p.Id,
                    (rp, p) => p)
                .Distinct() // Eliminamos duplicados en caso de que un permiso esté en múltiples roles
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task AssignPermissionToRoleAsync(Guid roleId, Guid permissionId)
        {
            if (roleId == Guid.Empty)
                throw new ArgumentException("El roleId no puede ser vacío", nameof(roleId));

            if (permissionId == Guid.Empty)
                throw new ArgumentException("El permissionId no puede ser vacío", nameof(permissionId));

            // Obtenemos el tenantId del rol
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
                throw new InvalidOperationException($"No se encontró el rol con ID {roleId}");

            // Verificamos si ya existe esta asignación
            bool exists = await _context.RolePermissions.AnyAsync(rp => 
                rp.Id == roleId && rp.PermissionId == permissionId);
                
            if (!exists)
            {
                // Si no existe, creamos la asignación
                await _context.RolePermissions.AddAsync(new RolePermission(roleId, permissionId, (role).TenantId));
            }
        }

        /// <inheritdoc/>
        public async Task RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId)
        {
            if (roleId == Guid.Empty)
                throw new ArgumentException("El roleId no puede ser vacío", nameof(roleId));

            if (permissionId == Guid.Empty)
                throw new ArgumentException("El permissionId no puede ser vacío", nameof(permissionId));

            // Buscamos la asignación de permiso al rol
            var rolePermission = await _context.RolePermissions.FirstOrDefaultAsync(rp => 
                rp.Id == roleId && rp.PermissionId == permissionId);
                
            if (rolePermission != null)
            {
                // Si existe, la eliminamos
                _context.RolePermissions.Remove(rolePermission);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionName)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El userId no puede ser vacío", nameof(userId));

            if (string.IsNullOrWhiteSpace(permissionName))
                throw new ArgumentException("El nombre del permiso no puede estar vacío", nameof(permissionName));

            // Verificamos si el usuario tiene el permiso a través de alguno de sus roles
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_context.RolePermissions,
                    ur => ur.RoleId,
                    rp => rp.Id,
                    (ur, rp) => rp)
                .Join(_context.Permissions,
                    rp => rp.PermissionId,
                    p => p.Id,
                    (rp, p) => p)
                .AnyAsync(p => p.Name == permissionName);
        }
    }
} 