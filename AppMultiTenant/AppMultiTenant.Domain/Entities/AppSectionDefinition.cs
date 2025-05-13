using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

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
            ValidateTenantId(tenantId);
            ValidateSectionName(name);
            ValidateDescription(description);
            
            AppSectionDefinitionId = Guid.NewGuid();
            TenantId = tenantId;
            Name = name;
            NormalizedName = NormalizeSectionName(name);
            Description = description;
            CreatedDate = DateTime.UtcNow;
            IsActive = true;
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
        /// Nombre normalizado de la sección, usado para generar identificadores consistentes.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string NormalizedName { get; private set; }
        
        /// <summary>
        /// Descripción opcional que explica el propósito o contenido de la sección.
        /// </summary>
        [StringLength(500)]
        public string Description { get; private set; }
        
        /// <summary>
        /// Indica si la sección está activa y puede ser utilizada para gestionar datos.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Fecha de creación de la definición de sección.
        /// </summary>
        public DateTime CreatedDate { get; private set; }

        /// <summary>
        /// Fecha de última modificación de la definición de sección.
        /// </summary>
        public DateTime? LastModifiedDate { get; private set; }

        /// <summary>
        /// Actualiza el nombre de la sección.
        /// </summary>
        /// <param name="name">Nuevo nombre.</param>
        public void UpdateName(string name)
        {
            ValidateSectionName(name);
            
            Name = name;
            NormalizedName = NormalizeSectionName(name);
            LastModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Actualiza la descripción de la sección.
        /// </summary>
        /// <param name="description">Nueva descripción.</param>
        public void UpdateDescription(string description)
        {
            ValidateDescription(description);
            
            Description = description;
            LastModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Activa la sección, permitiendo su uso.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
            LastModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Desactiva la sección, impidiendo nuevas operaciones sobre ella.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            LastModifiedDate = DateTime.UtcNow;
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
        /// Valida que el nombre de la sección sea válido.
        /// </summary>
        /// <param name="name">Nombre de la sección a validar.</param>
        /// <exception cref="ArgumentException">Si el nombre no cumple con los requisitos.</exception>
        private void ValidateSectionName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre de la sección no puede estar vacío", nameof(name));
            
            if (name.Length < 2 || name.Length > 100)
                throw new ArgumentException("El nombre de la sección debe tener entre 2 y 100 caracteres", nameof(name));
            
            // Permitir letras, números, espacios y algunos caracteres especiales
            if (!Regex.IsMatch(name, @"^[\p{L}\p{N}_\-\s\.]+$"))
                throw new ArgumentException("El nombre de la sección contiene caracteres no permitidos", nameof(name));
        }

        /// <summary>
        /// Valida que la descripción de la sección sea correcta.
        /// </summary>
        /// <param name="description">Descripción a validar.</param>
        /// <exception cref="ArgumentException">Si la descripción excede el límite permitido.</exception>
        private void ValidateDescription(string description)
        {
            if (description != null && description.Length > 500)
                throw new ArgumentException("La descripción no puede exceder 500 caracteres", nameof(description));
        }

        /// <summary>
        /// Normaliza el nombre de una sección para su uso en identificadores.
        /// </summary>
        /// <param name="name">Nombre a normalizar.</param>
        /// <returns>Nombre normalizado.</returns>
        private string NormalizeSectionName(string name)
        {
            // Eliminar espacios y caracteres especiales, convertir a PascalCase
            string normalized = Regex.Replace(name.Trim(), @"[\s\-\.]+", " ");
            normalized = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(normalized);
            normalized = Regex.Replace(normalized, @"[^\p{L}\p{N}]", "");
            return normalized;
        }

        /// <summary>
        /// Determina si esta sección puede ser modificada.
        /// </summary>
        /// <param name="hasData">Indica si la sección ya contiene datos.</param>
        /// <returns>True si la sección puede ser modificada, false de lo contrario.</returns>
        public bool CanBeModified(bool hasData)
        {
            // Si la sección ya tiene datos, las modificaciones deberían ser limitadas o requerir 
            // una migración de datos, pero para simplificar permitimos la modificación
            return true;
        }

        /// <summary>
        /// Determina si esta sección puede ser eliminada.
        /// </summary>
        /// <param name="hasData">Indica si la sección contiene datos.</param>
        /// <returns>True si la sección puede ser eliminada, false de lo contrario.</returns>
        public bool CanBeDeleted(bool hasData)
        {
            // No se debe permitir eliminar secciones que contengan datos
            return !hasData;
        }

        /// <summary>
        /// Genera el nombre del permiso para operaciones específicas en esta sección.
        /// </summary>
        /// <param name="operation">Tipo de operación (Create, Read, Update, Delete).</param>
        /// <returns>Nombre del permiso para la operación en esta sección.</returns>
        public string GetPermissionName(string operation)
        {
            if (string.IsNullOrWhiteSpace(operation))
                throw new ArgumentException("La operación no puede estar vacía", nameof(operation));
            
            return $"Can{operation}DataInSection{NormalizedName}";
        }
    }
} 