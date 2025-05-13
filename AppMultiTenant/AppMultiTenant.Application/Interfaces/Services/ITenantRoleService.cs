using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppMultiTenant.Domain.Entities;

namespace AppMultiTenant.Application.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio que maneja la gestión de roles y permisos dentro de un inquilino
    /// </summary>
    public interface ITenantRoleService
    {
        #region Roles
        
        /// <summary>
        /// Crea un nuevo rol dentro del inquilino
        /// </summary>
        /// <param name="name">Nombre del rol</param>
        /// <param name="description">Descripción del rol</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>El rol creado</returns>
        Task<ApplicationRole> CreateRoleAsync(string name, string description, Guid tenantId);
        
        /// <summary>
        /// Obtiene un rol por su Id, verificando que pertenezca al inquilino
        /// </summary>
        /// <param name="roleId">Id del rol</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>El rol si existe en el inquilino, null en caso contrario</returns>
        Task<ApplicationRole> GetRoleByIdAsync(Guid roleId, Guid tenantId);
        
        /// <summary>
        /// Obtiene un rol por su nombre, verificando que pertenezca al inquilino
        /// </summary>
        /// <param name="name">Nombre del rol</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>El rol si existe en el inquilino, null en caso contrario</returns>
        Task<ApplicationRole> GetRoleByNameAsync(string name, Guid tenantId);
        
        /// <summary>
        /// Obtiene todos los roles de un inquilino
        /// </summary>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>Lista de roles del inquilino</returns>
        Task<IEnumerable<ApplicationRole>> GetAllRolesAsync(Guid tenantId);
        
        /// <summary>
        /// Actualiza el nombre de un rol
        /// </summary>
        /// <param name="roleId">Id del rol</param>
        /// <param name="name">Nuevo nombre</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>El rol actualizado</returns>
        Task<ApplicationRole> UpdateRoleNameAsync(Guid roleId, string name, Guid tenantId);
        
        /// <summary>
        /// Actualiza la descripción de un rol
        /// </summary>
        /// <param name="roleId">Id del rol</param>
        /// <param name="description">Nueva descripción</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>El rol actualizado</returns>
        Task<ApplicationRole> UpdateRoleDescriptionAsync(Guid roleId, string description, Guid tenantId);
        
        /// <summary>
        /// Elimina un rol (si es posible)
        /// </summary>
        /// <param name="roleId">Id del rol</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>True si se eliminó correctamente, false si no se pudo eliminar</returns>
        Task<bool> DeleteRoleAsync(Guid roleId, Guid tenantId);
        
        /// <summary>
        /// Verifica si un nombre de rol está disponible dentro del inquilino
        /// </summary>
        /// <param name="name">Nombre a verificar</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <param name="excludeRoleId">Id de rol a excluir de la verificación (para ediciones)</param>
        /// <returns>True si el nombre está disponible, false si ya está en uso</returns>
        Task<bool> IsRoleNameAvailableAsync(string name, Guid tenantId, Guid? excludeRoleId = null);
        
        #endregion
        
        #region Permisos
        
        /// <summary>
        /// Obtiene todos los permisos del sistema
        /// </summary>
        /// <returns>Lista de todos los permisos</returns>
        Task<IEnumerable<Permission>> GetAllPermissionsAsync();
        
        /// <summary>
        /// Obtiene un permiso por su Id
        /// </summary>
        /// <param name="permissionId">Id del permiso</param>
        /// <returns>El permiso si existe, null en caso contrario</returns>
        Task<Permission> GetPermissionByIdAsync(Guid permissionId);
        
        /// <summary>
        /// Obtiene un permiso por su nombre
        /// </summary>
        /// <param name="name">Nombre del permiso</param>
        /// <returns>El permiso si existe, null en caso contrario</returns>
        Task<Permission> GetPermissionByNameAsync(string name);
        
        /// <summary>
        /// Obtiene los permisos asignados a un rol
        /// </summary>
        /// <param name="roleId">Id del rol</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>Lista de permisos asignados al rol</returns>
        Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId, Guid tenantId);
        
        /// <summary>
        /// Asigna permisos a un rol
        /// </summary>
        /// <param name="roleId">Id del rol</param>
        /// <param name="permissionIds">Lista de Ids de permisos a asignar</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>Lista de permisos asignados</returns>
        Task<IEnumerable<Permission>> AssignPermissionsToRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, Guid tenantId);
        
        /// <summary>
        /// Remueve permisos de un rol
        /// </summary>
        /// <param name="roleId">Id del rol</param>
        /// <param name="permissionIds">Lista de Ids de permisos a remover</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>True si la operación fue exitosa</returns>
        Task<bool> RemovePermissionsFromRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, Guid tenantId);
        
        /// <summary>
        /// Verifica si un usuario tiene un permiso específico
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="permissionName">Nombre del permiso</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>True si el usuario tiene el permiso, false en caso contrario</returns>
        Task<bool> UserHasPermissionAsync(Guid userId, string permissionName, Guid tenantId);
        
        /// <summary>
        /// Obtiene todos los permisos que tiene un usuario (a través de sus roles)
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>Lista de permisos del usuario</returns>
        Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId, Guid tenantId);
        
        #endregion
    }
} 