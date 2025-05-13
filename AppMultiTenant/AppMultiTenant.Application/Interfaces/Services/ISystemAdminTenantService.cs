using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppMultiTenant.Domain.Entities;

namespace AppMultiTenant.Application.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio que maneja la gestión de inquilinos por parte del Super Administrador
    /// </summary>
    public interface ISystemAdminTenantService
    {
        /// <summary>
        /// Crea un nuevo inquilino
        /// </summary>
        /// <param name="name">Nombre del inquilino</param>
        /// <param name="identifier">Identificador único para acceso (subdominio, etc.)</param>
        /// <returns>El inquilino creado</returns>
        Task<Tenant> CreateTenantAsync(string name, string identifier);
        
        /// <summary>
        /// Obtiene un inquilino por su Id
        /// </summary>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>El inquilino si existe, null en caso contrario</returns>
        Task<Tenant> GetTenantByIdAsync(Guid tenantId);
        
        /// <summary>
        /// Obtiene un inquilino por su identificador
        /// </summary>
        /// <param name="identifier">Identificador del inquilino</param>
        /// <returns>El inquilino si existe, null en caso contrario</returns>
        Task<Tenant> GetTenantByIdentifierAsync(string identifier);
        
        /// <summary>
        /// Obtiene todos los inquilinos 
        /// </summary>
        /// <param name="includeInactive">Indica si se deben incluir inquilinos inactivos</param>
        /// <param name="pageNumber">Número de página para paginación</param>
        /// <param name="pageSize">Tamaño de página para paginación</param>
        /// <returns>Lista paginada de inquilinos</returns>
        Task<(IEnumerable<Tenant> Tenants, int TotalCount)> GetAllTenantsAsync(bool includeInactive = false, int pageNumber = 1, int pageSize = 20);
        
        /// <summary>
        /// Actualiza el nombre de un inquilino
        /// </summary>
        /// <param name="tenantId">Id del inquilino</param>
        /// <param name="name">Nuevo nombre</param>
        /// <returns>El inquilino actualizado</returns>
        Task<Tenant> UpdateTenantNameAsync(Guid tenantId, string name);
        
        /// <summary>
        /// Actualiza el identificador de un inquilino
        /// </summary>
        /// <param name="tenantId">Id del inquilino</param>
        /// <param name="identifier">Nuevo identificador</param>
        /// <returns>El inquilino actualizado</returns>
        Task<Tenant> UpdateTenantIdentifierAsync(Guid tenantId, string identifier);
        
        /// <summary>
        /// Activa un inquilino
        /// </summary>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>El inquilino actualizado</returns>
        Task<Tenant> ActivateTenantAsync(Guid tenantId);
        
        /// <summary>
        /// Desactiva un inquilino
        /// </summary>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>El inquilino actualizado</returns>
        Task<Tenant> DeactivateTenantAsync(Guid tenantId);
        
        /// <summary>
        /// Elimina un inquilino completamente (operación peligrosa)
        /// </summary>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>True si se eliminó correctamente, false si no se pudo eliminar</returns>
        Task<bool> DeleteTenantAsync(Guid tenantId);
        
        /// <summary>
        /// Crea un administrador inicial para un inquilino recién creado
        /// </summary>
        /// <param name="tenantId">Id del inquilino</param>
        /// <param name="adminEmail">Email del administrador</param>
        /// <param name="adminUserName">Nombre de usuario del administrador</param>
        /// <param name="adminPassword">Contraseña inicial</param>
        /// <param name="adminFullName">Nombre completo del administrador</param>
        /// <returns>El usuario administrador creado</returns>
        Task<ApplicationUser> CreateInitialTenantAdminAsync(Guid tenantId, string adminEmail, string adminUserName, string adminPassword, string adminFullName);
        
        /// <summary>
        /// Verifica si un identificador de inquilino está disponible
        /// </summary>
        /// <param name="identifier">Identificador a verificar</param>
        /// <returns>True si está disponible, false si ya está en uso</returns>
        Task<bool> IsTenantIdentifierAvailableAsync(string identifier);
    }
} 