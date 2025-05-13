using System;
using System.ComponentModel.DataAnnotations;

namespace AppMultiTenant.Domain.Entities
{
    /// <summary>
    /// Representa la definición de una sección de aplicación personalizada para un inquilino.
    /// Cada sección puede contener múltiples campos definidos y almacenar datos estructurados.
    /// </summary>
    public class AppSectionDefinition : ITenantEntity
    {
        /// <summary>
        /// Constructor privado para EF Core.
        /// </summary>
        private AppSectionDefinition() { }

        /// <summary>
        /// Constructor para crear una nueva definición de sección.
        /// </summary>
        /// <param name="tenantId">ID del inquilino al que pertenece la sección.</param>
        /// <param name="name">Nombre de la sección.</param>
        /// <param name="description">Descripción opcional de la sección.</param>
        public AppSectionDefinition(Guid tenantId, string name, string description = null)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));
            
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre de la sección no puede estar vacío", nameof(name));
            
            if (name.Length < 2 || name.Length > 100)
                throw new ArgumentException("El nombre de la sección debe tener entre 2 y 100 caracteres", nameof(name));
            
            if (description != null && description.Length > 500)
                throw new ArgumentException("La descripción no puede exceder 500 caracteres", nameof(description));
            
            AppSectionDefinitionId = Guid.NewGuid();
            TenantId = tenantId;
            Name = name;
            Description = description;
            CreatedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Identificador único de la definición de sección.
        /// </summary>
        [Key]
        public Guid AppSectionDefinitionId { get; private set; }
        
        /// <summary>
        /// Identificador del inquilino al que pertenece esta definición de sección.
        /// </summary>
        [Required]
        public Guid TenantId { get; set; }
        
        /// <summary>
        /// Nombre de la sección, visible y utilizado por los usuarios del inquilino.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; private set; }
        
        /// <summary>
        /// Descripción opcional que explica el propósito o contenido de la sección.
        /// </summary>
        [StringLength(500)]
        public string Description { get; private set; }
        
        /// <summary>
        /// Fecha de creación de la definición de sección.
        /// </summary>
        public DateTime CreatedDate { get; private set; }

        /// <summary>
        /// Actualiza el nombre de la sección.
        /// </summary>
        /// <param name="name">Nuevo nombre.</param>
        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre de la sección no puede estar vacío", nameof(name));
            
            if (name.Length < 2 || name.Length > 100)
                throw new ArgumentException("El nombre de la sección debe tener entre 2 y 100 caracteres", nameof(name));
            
            Name = name;
        }

        /// <summary>
        /// Actualiza la descripción de la sección.
        /// </summary>
        /// <param name="description">Nueva descripción.</param>
        public void UpdateDescription(string description)
        {
            if (description != null && description.Length > 500)
                throw new ArgumentException("La descripción no puede exceder 500 caracteres", nameof(description));
            
            Description = description;
        }
    }
} 