using System;

namespace AppMultiTenant.Domain.Entities
{
    /// <summary>
    /// Interfaz que deben implementar todas las entidades que pertenecen a un inquilino específico.
    /// Permite aplicar filtrado global y asignación automática del TenantId.
    /// </summary>
    public interface ITenantEntity
    {
        /// <summary>
        /// Identificador del inquilino al que pertenece esta entidad.
        /// </summary>
        Guid TenantId { get; set; }
    }
} 