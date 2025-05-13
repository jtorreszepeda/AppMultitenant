using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
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
        private const string ADMIN_ROLE_NAME = "Administrador";

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
            
            ValidateRoleName(name);
            ValidateTenantId(tenantId);
            ValidateDescription(description);
            
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
        /// Última fecha de modificación del rol.
        /// </summary>
        public DateTime? LastModifiedDate { get; private set; }

        /// <summary>
        /// Actualiza la descripción del rol.
        /// </summary>
        /// <param name="description">Nueva descripción.</param>
        public void UpdateDescription(string description)
        {
            ValidateDescription(description);
            Description = description;
            LastModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Actualiza el nombre del rol.
        /// </summary>
        /// <param name="name">Nuevo nombre del rol.</param>
        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre del rol no puede estar vacío", nameof(name));
            
            // No permitir cambiar el nombre del rol de administrador
            if (IsAdminRole() && name != ADMIN_ROLE_NAME)
                throw new InvalidOperationException("No se puede cambiar el nombre del rol de Administrador");
            
            ValidateRoleName(name);
            
            Name = name;
            NormalizedName = name.ToUpperInvariant();
            LastModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Activa el rol.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
            LastModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Desactiva el rol.
        /// </summary>
        public void Deactivate()
        {
            // No permitir desactivar el rol de administrador
            if (IsAdminRole())
                throw new InvalidOperationException("No se puede desactivar el rol de Administrador");
            
            IsActive = false;
            LastModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Valida que el formato del nombre del rol sea correcto.
        /// </summary>
        /// <param name="name">Nombre del rol a validar.</param>
        /// <exception cref="ArgumentException">Si el nombre no cumple con los requisitos.</exception>
        private void ValidateRoleName(string name)
        {
            if (name.Length < 2 || name.Length > 50)
                throw new ArgumentException("El nombre del rol debe tener entre 2 y 50 caracteres", nameof(name));
            
            // Permitir caracteres alfanuméricos, espacios y algunos caracteres especiales
            if (!Regex.IsMatch(name, @"^[\p{L}\p{N}_\-\s\.]+$"))
                throw new ArgumentException("El nombre del rol contiene caracteres no permitidos", nameof(name));
        }

        /// <summary>
        /// Valida que el TenantId sea válido.
        /// </summary>
        /// <param name="tenantId">ID del inquilino a validar.</param>
        /// <exception cref="ArgumentException">Si el TenantId no es válido.</exception>
        private void ValidateTenantId(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));
        }

        /// <summary>
        /// Valida que la descripción del rol sea correcta.
        /// </summary>
        /// <param name="description">Descripción a validar.</param>
        /// <exception cref="ArgumentException">Si la descripción excede el límite permitido.</exception>
        private void ValidateDescription(string description)
        {
            if (description != null && description.Length > 500)
            {
                throw new ArgumentException("La descripción no puede exceder los 500 caracteres", nameof(description));
            }
        }

        /// <summary>
        /// Verifica si este rol es el rol de Administrador.
        /// </summary>
        /// <returns>True si es el rol de Administrador, false de lo contrario.</returns>
        public bool IsAdminRole()
        {
            return Name == ADMIN_ROLE_NAME || NormalizedName == ADMIN_ROLE_NAME.ToUpperInvariant();
        }

        /// <summary>
        /// Determina si el rol puede ser eliminado.
        /// </summary>
        /// <param name="hasUsers">Indica si hay usuarios asignados a este rol.</param>
        /// <returns>True si el rol puede ser eliminado, false de lo contrario.</returns>
        public bool CanBeDeleted(bool hasUsers)
        {
            // No se permite eliminar el rol de administrador ni roles con usuarios asignados
            return !IsAdminRole() && !hasUsers;
        }
    }
} 