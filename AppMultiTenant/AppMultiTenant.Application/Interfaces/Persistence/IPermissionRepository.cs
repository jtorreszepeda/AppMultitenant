using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppMultiTenant.Domain.Entities;

namespace AppMultiTenant.Application.Interfaces.Persistence
{
    /// <summary>
    /// Interfaz para el repositorio de permisos del sistema
    /// </summary>
    public interface IPermissionRepository : IRepositoryBase<Permission>
    {
        /// <summary>
        /// Obtiene un permiso por su nombre
        /// </summary>
        /// <param name="name">Nombre del permiso</param>
        /// <returns>El permiso si existe, null en caso contrario</returns>
        Task<Permission> GetByNameAsync(string name);
        
        /// <summary>
        /// Obtiene todos los permisos asignados a un rol específico
        /// </summary>
        /// <param name="roleId">Id del rol</param>
        /// <returns>Lista de permisos asignados al rol</returns>
        Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId);
        
        /// <summary>
        /// Obtiene todos los permisos asignados a los roles de un usuario
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <returns>Lista de permisos del usuario a través de sus roles</returns>
        Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(Guid userId);
        
        /// <summary>
        /// Asigna un permiso a un rol
        /// </summary>
        /// <param name="roleId">Id del rol</param>
        /// <param name="permissionId">Id del permiso</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task AssignPermissionToRoleAsync(Guid roleId, Guid permissionId);
        
        /// <summary>
        /// Revoca un permiso de un rol
        /// </summary>
        /// <param name="roleId">Id del rol</param>
        /// <param name="permissionId">Id del permiso</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId);
        
        /// <summary>
        /// Verifica si un usuario tiene un permiso específico
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="permissionName">Nombre del permiso</param>
        /// <returns>True si tiene el permiso, False en caso contrario</returns>
        Task<bool> UserHasPermissionAsync(Guid userId, string permissionName);
    }
} 