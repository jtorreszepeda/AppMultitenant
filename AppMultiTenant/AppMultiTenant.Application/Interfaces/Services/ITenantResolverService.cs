using System;

namespace AppMultiTenant.Application.Interfaces.Services
{
    /// <summary>
    /// Servicio para resolver el inquilino actual en el contexto de una solicitud.
    /// Es utilizado por el DbContext para aplicar filtros de inquilino.
    /// </summary>
    public interface ITenantResolverService
    {
        /// <summary>
        /// Obtiene el identificador del inquilino actual.
        /// Retorna null para operaciones a nivel SuperAdmin que no tienen un inquilino específico.
        /// </summary>
        /// <returns>El GUID del inquilino actual o null si es SuperAdmin</returns>
        Guid? GetCurrentTenantId();
        
        /// <summary>
        /// Obtiene el identificador (nombre único) del inquilino actual.
        /// </summary>
        /// <returns>El identificador del inquilino actual o null si es SuperAdmin</returns>
        string? GetCurrentTenantIdentifier();
    }
} 