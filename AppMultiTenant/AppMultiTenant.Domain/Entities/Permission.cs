using System;
using System.ComponentModel.DataAnnotations;

namespace AppMultiTenant.Domain.Entities
{
    /// <summary>
    /// Representa un permiso en el sistema.
    /// Los permisos definen las acciones específicas que los usuarios pueden realizar en la aplicación.
    /// </summary>
    public class Permission
    {
        /// <summary>
        /// Constructor privado para EF Core.
        /// </summary>
        private Permission() { }

        /// <summary>
        /// Constructor para crear un nuevo permiso.
        /// </summary>
        /// <param name="name">Nombre único del permiso.</param>
        /// <param name="description">Descripción detallada del permiso.</param>
        public Permission(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre del permiso no puede estar vacío", nameof(name));
            
            if (name.Length < 3 || name.Length > 100)
                throw new ArgumentException("El nombre del permiso debe tener entre 3 y 100 caracteres", nameof(name));
            
            PermissionId = Guid.NewGuid();
            Name = name;
            Description = description ?? string.Empty;
            CreatedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Identificador único del permiso.
        /// </summary>
        [Key]
        public Guid PermissionId { get; private set; }
        
        /// <summary>
        /// Nombre único del permiso, utilizado para identificarlo en el sistema.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; private set; }
        
        /// <summary>
        /// Descripción detallada que explica el propósito del permiso.
        /// </summary>
        [StringLength(500)]
        public string Description { get; private set; }
        
        /// <summary>
        /// Fecha de creación del permiso.
        /// </summary>
        public DateTime CreatedDate { get; private set; }

        /// <summary>
        /// Actualiza el nombre del permiso.
        /// </summary>
        /// <param name="name">Nuevo nombre.</param>
        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre del permiso no puede estar vacío", nameof(name));
            
            if (name.Length < 3 || name.Length > 100)
                throw new ArgumentException("El nombre del permiso debe tener entre 3 y 100 caracteres", nameof(name));
            
            Name = name;
        }

        /// <summary>
        /// Actualiza la descripción del permiso.
        /// </summary>
        /// <param name="description">Nueva descripción.</param>
        public void UpdateDescription(string description)
        {
            Description = description ?? string.Empty;
        }
    }
} 