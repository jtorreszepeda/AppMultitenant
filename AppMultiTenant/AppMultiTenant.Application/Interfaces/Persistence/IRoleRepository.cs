using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppMultiTenant.Domain.Entities;

namespace AppMultiTenant.Application.Interfaces.Persistence
{
    /// <summary>
    /// Interfaz para el repositorio de roles de inquilino
    /// </summary>
    public interface IRoleRepository : IRepositoryBase<ApplicationRole>
    {
        /// <summary>
        /// Obtiene un rol por su nombre dentro de un inquilino específico
        /// </summary>
        /// <param name="roleName">Nombre del rol</param>
        /// <param name="tenantId">Id del inquilino al que pertenece</param>
        /// <returns>El rol si existe, null en caso contrario</returns>
        Task<ApplicationRole> GetByNameAsync(string roleName, Guid tenantId);
        
        /// <summary>
        /// Obtiene todos los roles pertenecientes a un inquilino específico
        /// </summary>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>Lista de roles del inquilino</returns>
        Task<IEnumerable<ApplicationRole>> GetAllByTenantIdAsync(Guid tenantId);
        
        /// <summary>
        /// Obtiene los roles asignados a un usuario específico
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <returns>Lista de roles asignados al usuario</returns>
        Task<IEnumerable<ApplicationRole>> GetRolesByUserIdAsync(Guid userId);
        
        /// <summary>
        /// Asigna un rol a un usuario
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="roleId">Id del rol</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task AssignRoleToUserAsync(Guid userId, Guid roleId);
        
        /// <summary>
        /// Revoca un rol de un usuario
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="roleId">Id del rol</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task RemoveRoleFromUserAsync(Guid userId, Guid roleId);
    }
} 