using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppMultiTenant.Domain.Entities;

namespace AppMultiTenant.Application.Interfaces.Persistence
{
    /// <summary>
    /// Interfaz para el repositorio de definiciones de secciones de aplicación
    /// </summary>
    public interface IAppSectionDefinitionRepository : IRepositoryBase<AppSectionDefinition>
    {
        /// <summary>
        /// Obtiene una definición de sección por su nombre dentro de un inquilino
        /// </summary>
        /// <param name="name">Nombre de la sección</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>La definición de sección si existe, null en caso contrario</returns>
        Task<AppSectionDefinition> GetByNameAsync(string name, Guid tenantId);
        
        /// <summary>
        /// Obtiene todas las definiciones de secciones de un inquilino
        /// </summary>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>Lista de definiciones de secciones del inquilino</returns>
        Task<IEnumerable<AppSectionDefinition>> GetAllByTenantIdAsync(Guid tenantId);
        
        /// <summary>
        /// Comprueba si existe una sección con el nombre dado en un inquilino
        /// </summary>
        /// <param name="name">Nombre de la sección</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>True si existe, False en caso contrario</returns>
        Task<bool> ExistsByNameInTenantAsync(string name, Guid tenantId);
        
        /// <summary>
        /// Obtiene las definiciones de secciones a las que un usuario tiene acceso según sus permisos
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>Lista de definiciones de secciones accesibles</returns>
        Task<IEnumerable<AppSectionDefinition>> GetAccessibleSectionsForUserAsync(Guid userId, Guid tenantId);
    }
} 