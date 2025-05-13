using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using AppMultiTenant.Domain.Entities;

namespace AppMultiTenant.Application.Interfaces.Persistence
{
    /// <summary>
    /// Interfaz para el repositorio de inquilinos (Tenants)
    /// </summary>
    public interface ITenantRepository : IRepositoryBase<Tenant>
    {
        /// <summary>
        /// Obtiene un inquilino por su identificador único (subdominio, ruta, etc.)
        /// </summary>
        /// <param name="identifier">Identificador único del inquilino</param>
        /// <returns>El inquilino si existe, null en caso contrario</returns>
        Task<Tenant> GetByIdentifierAsync(string identifier);
        
        /// <summary>
        /// Verifica si un identificador de inquilino está disponible (no existe)
        /// </summary>
        /// <param name="identifier">Identificador a verificar</param>
        /// <returns>True si está disponible, False si ya existe</returns>
        Task<bool> IsIdentifierAvailableAsync(string identifier);
        
        /// <summary>
        /// Obtiene la lista de inquilinos activos
        /// </summary>
        /// <returns>Lista de inquilinos activos</returns>
        Task<IEnumerable<Tenant>> GetAllActiveAsync();
    }
} 