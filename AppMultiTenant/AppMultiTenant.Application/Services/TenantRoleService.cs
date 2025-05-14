using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppMultiTenant.Application.Interfaces.Persistence;
using AppMultiTenant.Application.Interfaces.Services;
using AppMultiTenant.Domain.Entities;

namespace AppMultiTenant.Application.Services
{
    /// <summary>
    /// Implementación del servicio que gestiona roles y permisos dentro de un inquilino
    /// </summary>
    public class TenantRoleService : ITenantRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        /// <param name="roleRepository">Repositorio de roles</param>
        /// <param name="permissionRepository">Repositorio de permisos</param>
        /// <param name="unitOfWork">Unidad de trabajo para transacciones</param>
        public TenantRoleService(
            IRoleRepository roleRepository,
            IPermissionRepository permissionRepository,
            IUnitOfWork unitOfWork)
        {
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        #region Roles

        /// <inheritdoc/>
        public async Task<ApplicationRole> CreateRoleAsync(string name, string description, Guid tenantId)
        {
            // Validar que el nombre no esté en uso
            bool isNameAvailable = await IsRoleNameAvailableAsync(name, tenantId);
            if (!isNameAvailable)
            {
                throw new InvalidOperationException($"El nombre de rol '{name}' ya está en uso en este inquilino");
            }

            // Crear el nuevo rol
            var newRole = new ApplicationRole(name, tenantId, description);
            
            // Agregar al repositorio
            await _roleRepository.AddAsync(newRole);
            await _unitOfWork.SaveChangesAsync();
            
            return newRole;
        }

        /// <inheritdoc/>
        public async Task<ApplicationRole> GetRoleByIdAsync(Guid roleId, Guid tenantId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            
            // Verificar que el rol pertenezca al inquilino
            if (role == null || role.TenantId != tenantId)
            {
                return null;
            }
            
            return role;
        }

        /// <inheritdoc/>
        public async Task<ApplicationRole> GetRoleByNameAsync(string name, Guid tenantId)
        {
            return await _roleRepository.GetByNameAsync(name, tenantId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationRole>> GetAllRolesAsync(Guid tenantId)
        {
            return await _roleRepository.GetAllByTenantIdAsync(tenantId);
        }

        /// <inheritdoc/>
        public async Task<ApplicationRole> UpdateRoleNameAsync(Guid roleId, string name, Guid tenantId)
        {
            // Obtener el rol y verificar que pertenezca al inquilino
            var role = await GetRoleByIdAsync(roleId, tenantId);
            if (role == null)
            {
                throw new InvalidOperationException($"No se encontró el rol con ID {roleId} en este inquilino");
            }
            
            // Verificar que el nuevo nombre no esté en uso (excluyendo el rol actual)
            bool isNameAvailable = await IsRoleNameAvailableAsync(name, tenantId, roleId);
            if (!isNameAvailable)
            {
                throw new InvalidOperationException($"El nombre de rol '{name}' ya está en uso en este inquilino");
            }
            
            // Actualizar el nombre
            role.UpdateName(name);
            
            // Guardar cambios
            await _roleRepository.UpdateAsync(role);
            await _unitOfWork.SaveChangesAsync();
            
            return role;
        }

        /// <inheritdoc/>
        public async Task<ApplicationRole> UpdateRoleDescriptionAsync(Guid roleId, string description, Guid tenantId)
        {
            // Obtener el rol y verificar que pertenezca al inquilino
            var role = await GetRoleByIdAsync(roleId, tenantId);
            if (role == null)
            {
                throw new InvalidOperationException($"No se encontró el rol con ID {roleId} en este inquilino");
            }
            
            // Actualizar la descripción
            role.UpdateDescription(description);
            
            // Guardar cambios
            await _roleRepository.UpdateAsync(role);
            await _unitOfWork.SaveChangesAsync();
            
            return role;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteRoleAsync(Guid roleId, Guid tenantId)
        {
            // Obtener el rol y verificar que pertenezca al inquilino
            var role = await GetRoleByIdAsync(roleId, tenantId);
            if (role == null)
            {
                throw new InvalidOperationException($"No se encontró el rol con ID {roleId} en este inquilino");
            }
            
            // Verificar si hay usuarios asignados al rol
            var usersWithRole = await _roleRepository.GetRolesByUserIdAsync(roleId);
            bool hasUsers = usersWithRole.Any();
            
            // Verificar si el rol puede ser eliminado
            if (!role.CanBeDeleted(hasUsers))
            {
                return false;
            }
            
            // Eliminar todas las asignaciones de permisos al rol
            var permissions = await _permissionRepository.GetPermissionsByRoleIdAsync(roleId);
            foreach (var permission in permissions)
            {
                await _permissionRepository.RemovePermissionFromRoleAsync(roleId, permission.Id);
            }
            
            // Eliminar el rol
            await _roleRepository.RemoveAsync(role);
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> IsRoleNameAvailableAsync(string name, Guid tenantId, Guid? excludeRoleId = null)
        {
            var existingRole = await _roleRepository.GetByNameAsync(name, tenantId);
            
            // Si no existe un rol con ese nombre, está disponible
            if (existingRole == null)
            {
                return true;
            }
            
            // Si existe pero es el mismo rol que estamos editando, está disponible
            if (excludeRoleId.HasValue && existingRole.Id == excludeRoleId.Value)
            {
                return true;
            }
            
            // En cualquier otro caso, no está disponible
            return false;
        }

        #endregion

        #region Permisos

        /// <inheritdoc/>
        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            return await _permissionRepository.GetAllAsync();
        }

        /// <inheritdoc/>
        public async Task<Permission> GetPermissionByIdAsync(Guid permissionId)
        {
            return await _permissionRepository.GetByIdAsync(permissionId);
        }

        /// <inheritdoc/>
        public async Task<Permission> GetPermissionByNameAsync(string name)
        {
            return await _permissionRepository.GetByNameAsync(name);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId, Guid tenantId)
        {
            // Verificar que el rol pertenezca al inquilino
            var role = await GetRoleByIdAsync(roleId, tenantId);
            if (role == null)
            {
                throw new InvalidOperationException($"No se encontró el rol con ID {roleId} en este inquilino");
            }
            
            // Obtener los permisos asignados al rol
            return await _permissionRepository.GetPermissionsByRoleIdAsync(roleId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Permission>> AssignPermissionsToRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, Guid tenantId)
        {
            // Verificar que el rol pertenezca al inquilino
            var role = await GetRoleByIdAsync(roleId, tenantId);
            if (role == null)
            {
                throw new InvalidOperationException($"No se encontró el rol con ID {roleId} en este inquilino");
            }
            
            // Obtener los permisos actuales del rol
            var currentPermissions = await _permissionRepository.GetPermissionsByRoleIdAsync(roleId);
            var currentPermissionIds = currentPermissions.Select(p => p.Id).ToHashSet();
            
            // Filtrar solo los permisos que no están ya asignados
            var permissionsToAdd = permissionIds.Where(id => !currentPermissionIds.Contains(id)).ToList();
            
            // Verificar que todos los permisos existan
            foreach (var permissionId in permissionsToAdd)
            {
                var permission = await _permissionRepository.GetByIdAsync(permissionId);
                if (permission == null)
                {
                    throw new InvalidOperationException($"No se encontró el permiso con ID {permissionId}");
                }
                
                // Asignar el permiso al rol
                await _permissionRepository.AssignPermissionToRoleAsync(roleId, permissionId);
            }
            
            await _unitOfWork.SaveChangesAsync();
            
            // Retornar la lista actualizada de permisos
            return await _permissionRepository.GetPermissionsByRoleIdAsync(roleId);
        }

        /// <inheritdoc/>
        public async Task<bool> RemovePermissionsFromRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, Guid tenantId)
        {
            // Verificar que el rol pertenezca al inquilino
            var role = await GetRoleByIdAsync(roleId, tenantId);
            if (role == null)
            {
                throw new InvalidOperationException($"No se encontró el rol con ID {roleId} en este inquilino");
            }
            
            // Verificar que no se intente remover permisos esenciales del rol de Administrador
            if (role.IsAdminRole())
            {
                // Aquí podríamos implementar una lógica para evitar remover permisos críticos
                // del rol de Administrador, si fuera necesario
            }
            
            foreach (var permissionId in permissionIds)
            {
                await _permissionRepository.RemovePermissionFromRoleAsync(roleId, permissionId);
            }
            
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionName, Guid tenantId)
        {
            // Validaciones
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("El ID del usuario no puede estar vacío", nameof(userId));
            }
            
            if (string.IsNullOrWhiteSpace(permissionName))
            {
                throw new ArgumentException("El nombre del permiso no puede estar vacío", nameof(permissionName));
            }
            
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));
            }
            
            return await _permissionRepository.UserHasPermissionAsync(userId, permissionName);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId, Guid tenantId)
        {
            // Validaciones
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("El ID del usuario no puede estar vacío", nameof(userId));
            }
            
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));
            }
            
            return await _permissionRepository.GetPermissionsByUserIdAsync(userId);
        }

        #endregion
    }
} 