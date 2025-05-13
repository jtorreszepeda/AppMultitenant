using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AppMultiTenant.Domain.Entities
{
    /// <summary>
    /// Representa un rol en el sistema multi-inquilino.
    /// Extiende IdentityRole<Guid> e implementa ITenantEntity para permitir
    /// el filtrado automático por inquilino.
    /// </summary>
    public class ApplicationRole : IdentityRole<Guid>, ITenantEntity
    {
        /// <summary>
        /// Constructor privado para EF Core.
        /// </summary>
        private ApplicationRole() { }

        /// <summary>
        /// Constructor para crear un nuevo rol asociado a un inquilino.
        /// </summary>
        /// <param name="name">Nombre del rol.</param>
        /// <param name="tenantId">ID del inquilino al que pertenece.</param>
        /// <param name="description">Descripción opcional del rol.</param>
        public ApplicationRole(string name, Guid tenantId, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre del rol no puede estar vacío", nameof(name));
            
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));
            
            Id = Guid.NewGuid();
            Name = name;
            NormalizedName = name.ToUpperInvariant();
            TenantId = tenantId;
            Description = description;
            IsActive = true;
            CreatedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Identificador del inquilino al que pertenece este rol.
        /// </summary>
        [Required]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Descripción detallada del rol y sus responsabilidades.
        /// </summary>
        [StringLength(500)]
        public string Description { get; private set; }

        /// <summary>
        /// Indica si el rol está activo y puede ser asignado a usuarios.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Fecha de creación del rol.
        /// </summary>
        public DateTime CreatedDate { get; private set; }

        /// <summary>
        /// Actualiza la descripción del rol.
        /// </summary>
        /// <param name="description">Nueva descripción.</param>
        public void UpdateDescription(string description)
        {
            if (description != null && description.Length > 500)
            {
                throw new ArgumentException("La descripción no puede exceder los 500 caracteres", nameof(description));
            }
            
            Description = description;
        }

        /// <summary>
        /// Actualiza el nombre del rol.
        /// </summary>
        /// <param name="name">Nuevo nombre del rol.</param>
        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre del rol no puede estar vacío", nameof(name));
            
            Name = name;
            NormalizedName = name.ToUpperInvariant();
        }

        /// <summary>
        /// Activa el rol.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Desactiva el rol.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }
    }
} 