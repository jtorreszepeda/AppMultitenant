using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppMultiTenant.Domain.Entities;

namespace AppMultiTenant.Application.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio que maneja la gestión de definiciones de secciones dentro de un inquilino
    /// </summary>
    public interface ITenantSectionDefinitionService
    {
        #region Métodos con TenantId automático
        
        /// <summary>
        /// Crea una nueva definición de sección dentro del inquilino actual
        /// </summary>
        /// <param name="name">Nombre de la sección</param>
        /// <param name="description">Descripción de la sección</param>
        /// <returns>La definición de sección creada</returns>
        Task<AppSectionDefinition> CreateSectionDefinitionAsync(string name, string description);
        
        /// <summary>
        /// Obtiene una definición de sección por su Id, verificando que pertenezca al inquilino actual
        /// </summary>
        /// <param name="sectionDefinitionId">Id de la definición de sección</param>
        /// <returns>La definición de sección si existe en el inquilino, null en caso contrario</returns>
        Task<AppSectionDefinition> GetSectionDefinitionByIdAsync(Guid sectionDefinitionId);
        
        /// <summary>
        /// Obtiene una definición de sección por su nombre, verificando que pertenezca al inquilino actual
        /// </summary>
        /// <param name="name">Nombre de la sección</param>
        /// <returns>La definición de sección si existe en el inquilino, null en caso contrario</returns>
        Task<AppSectionDefinition> GetSectionDefinitionByNameAsync(string name);
        
        /// <summary>
        /// Obtiene todas las definiciones de sección del inquilino actual
        /// </summary>
        /// <returns>Lista de definiciones de sección del inquilino</returns>
        Task<IEnumerable<AppSectionDefinition>> GetAllSectionDefinitionsAsync();
        
        /// <summary>
        /// Actualiza el nombre de una definición de sección del inquilino actual
        /// </summary>
        /// <param name="sectionDefinitionId">Id de la definición de sección</param>
        /// <param name="name">Nuevo nombre</param>
        /// <returns>La definición de sección actualizada</returns>
        Task<AppSectionDefinition> UpdateSectionDefinitionNameAsync(Guid sectionDefinitionId, string name);
        
        /// <summary>
        /// Actualiza la descripción de una definición de sección del inquilino actual
        /// </summary>
        /// <param name="sectionDefinitionId">Id de la definición de sección</param>
        /// <param name="description">Nueva descripción</param>
        /// <returns>La definición de sección actualizada</returns>
        Task<AppSectionDefinition> UpdateSectionDefinitionDescriptionAsync(Guid sectionDefinitionId, string description);
        
        /// <summary>
        /// Elimina una definición de sección del inquilino actual (si es posible)
        /// </summary>
        /// <param name="sectionDefinitionId">Id de la definición de sección</param>
        /// <param name="force">Indica si se debe forzar la eliminación incluso si hay datos asociados</param>
        /// <returns>True si se eliminó correctamente, false si no se pudo eliminar</returns>
        Task<bool> DeleteSectionDefinitionAsync(Guid sectionDefinitionId, bool force = false);
        
        /// <summary>
        /// Verifica si un nombre de sección está disponible dentro del inquilino actual
        /// </summary>
        /// <param name="name">Nombre a verificar</param>
        /// <param name="excludeSectionDefinitionId">Id de definición de sección a excluir de la verificación (para ediciones)</param>
        /// <returns>True si el nombre está disponible, false si ya está en uso</returns>
        Task<bool> IsSectionNameAvailableAsync(string name, Guid? excludeSectionDefinitionId = null);
        
        /// <summary>
        /// Verifica si una definición de sección puede ser eliminada
        /// </summary>
        /// <param name="sectionDefinitionId">Id de la definición de sección</param>
        /// <returns>True si puede ser eliminada, false en caso contrario</returns>
        Task<bool> CanDeleteSectionDefinitionAsync(Guid sectionDefinitionId);
        
        /// <summary>
        /// Crea y asigna permisos para una nueva sección en el inquilino actual
        /// </summary>
        /// <param name="sectionDefinitionId">Id de la definición de sección</param>
        /// <param name="sectionName">Nombre de la sección</param>
        /// <param name="adminRoleId">Id del rol de administrador para asignar permisos automáticamente</param>
        /// <returns>Lista de permisos creados</returns>
        Task<IEnumerable<Permission>> CreateAndAssignSectionPermissionsAsync(
            Guid sectionDefinitionId, string sectionName, Guid adminRoleId);
        
        #endregion
        
        #region Métodos con TenantId explícito
        
        /// <summary>
        /// Crea una nueva definición de sección dentro del inquilino
        /// </summary>
        /// <param name="name">Nombre de la sección</param>
        /// <param name="description">Descripción de la sección</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>La definición de sección creada</returns>
        Task<AppSectionDefinition> CreateSectionDefinitionAsync(string name, string description, Guid tenantId);
        
        /// <summary>
        /// Obtiene una definición de sección por su Id, verificando que pertenezca al inquilino
        /// </summary>
        /// <param name="sectionDefinitionId">Id de la definición de sección</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>La definición de sección si existe en el inquilino, null en caso contrario</returns>
        Task<AppSectionDefinition> GetSectionDefinitionByIdAsync(Guid sectionDefinitionId, Guid tenantId);
        
        /// <summary>
        /// Obtiene una definición de sección por su nombre, verificando que pertenezca al inquilino
        /// </summary>
        /// <param name="name">Nombre de la sección</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>La definición de sección si existe en el inquilino, null en caso contrario</returns>
        Task<AppSectionDefinition> GetSectionDefinitionByNameAsync(string name, Guid tenantId);
        
        /// <summary>
        /// Obtiene todas las definiciones de sección de un inquilino
        /// </summary>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>Lista de definiciones de sección del inquilino</returns>
        Task<IEnumerable<AppSectionDefinition>> GetAllSectionDefinitionsAsync(Guid tenantId);
        
        /// <summary>
        /// Actualiza el nombre de una definición de sección
        /// </summary>
        /// <param name="sectionDefinitionId">Id de la definición de sección</param>
        /// <param name="name">Nuevo nombre</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>La definición de sección actualizada</returns>
        Task<AppSectionDefinition> UpdateSectionDefinitionNameAsync(Guid sectionDefinitionId, string name, Guid tenantId);
        
        /// <summary>
        /// Actualiza la descripción de una definición de sección
        /// </summary>
        /// <param name="sectionDefinitionId">Id de la definición de sección</param>
        /// <param name="description">Nueva descripción</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>La definición de sección actualizada</returns>
        Task<AppSectionDefinition> UpdateSectionDefinitionDescriptionAsync(Guid sectionDefinitionId, string description, Guid tenantId);
        
        /// <summary>
        /// Elimina una definición de sección (si es posible)
        /// </summary>
        /// <param name="sectionDefinitionId">Id de la definición de sección</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <param name="force">Indica si se debe forzar la eliminación incluso si hay datos asociados</param>
        /// <returns>True si se eliminó correctamente, false si no se pudo eliminar</returns>
        Task<bool> DeleteSectionDefinitionAsync(Guid sectionDefinitionId, Guid tenantId, bool force = false);
        
        /// <summary>
        /// Verifica si un nombre de sección está disponible dentro del inquilino
        /// </summary>
        /// <param name="name">Nombre a verificar</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <param name="excludeSectionDefinitionId">Id de definición de sección a excluir de la verificación (para ediciones)</param>
        /// <returns>True si el nombre está disponible, false si ya está en uso</returns>
        Task<bool> IsSectionNameAvailableAsync(string name, Guid tenantId, Guid? excludeSectionDefinitionId = null);
        
        /// <summary>
        /// Verifica si una definición de sección puede ser eliminada
        /// </summary>
        /// <param name="sectionDefinitionId">Id de la definición de sección</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>True si puede ser eliminada, false en caso contrario</returns>
        Task<bool> CanDeleteSectionDefinitionAsync(Guid sectionDefinitionId, Guid tenantId);
        
        /// <summary>
        /// Crea y asigna permisos para una nueva sección
        /// </summary>
        /// <param name="sectionDefinitionId">Id de la definición de sección</param>
        /// <param name="sectionName">Nombre de la sección</param>
        /// <param name="adminRoleId">Id del rol de administrador para asignar permisos automáticamente</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>Lista de permisos creados</returns>
        Task<IEnumerable<Permission>> CreateAndAssignSectionPermissionsAsync(Guid sectionDefinitionId, string sectionName, Guid adminRoleId, Guid tenantId);
        
        #endregion
    }
} 