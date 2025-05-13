using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppMultiTenant.Domain.Entities;

namespace AppMultiTenant.Application.Interfaces.Persistence
{
    /// <summary>
    /// Interfaz para el repositorio de usuarios de inquilino
    /// </summary>
    public interface IUserRepository : IRepositoryBase<ApplicationUser>
    {
        /// <summary>
        /// Obtiene un usuario por su email y tenantId
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="tenantId">Id del inquilino al que pertenece</param>
        /// <returns>El usuario si existe, null en caso contrario</returns>
        Task<ApplicationUser> GetByEmailAsync(string email, Guid tenantId);

        /// <summary>
        /// Obtiene todos los usuarios pertenecientes a un inquilino específico
        /// </summary>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>Lista de usuarios del inquilino</returns>
        Task<IEnumerable<ApplicationUser>> GetAllByTenantIdAsync(Guid tenantId);
        
        /// <summary>
        /// Obtiene todos los usuarios activos de un inquilino
        /// </summary>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>Lista de usuarios activos del inquilino</returns>
        Task<IEnumerable<ApplicationUser>> GetAllActiveByTenantIdAsync(Guid tenantId);
        
        /// <summary>
        /// Verifica si existe algún usuario con el rol de administrador en el inquilino
        /// </summary>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>True si existe al menos un administrador, False en caso contrario</returns>
        Task<bool> ExistsAdminInTenantAsync(Guid tenantId);
    }
} 