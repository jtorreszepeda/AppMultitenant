using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AppMultiTenant.Domain.Entities
{
    /// <summary>
    /// Representa un inquilino (tenant) en el sistema multi-inquilino.
    /// Cada inquilino tiene su propio espacio aislado en la aplicación.
    /// </summary>
    public class Tenant
    {
        /// <summary>
        /// Constructor privado para EF Core.
        /// </summary>
        private Tenant() { }

        /// <summary>
        /// Constructor para crear un nuevo inquilino.
        /// </summary>
        /// <param name="name">Nombre del inquilino.</param>
        /// <param name="identifier">Identificador único para acceso.</param>
        public Tenant(string name, string identifier)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre del inquilino no puede estar vacío", nameof(name));
            
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("El identificador del inquilino no puede estar vacío", nameof(identifier));
            
            if (name.Length < 2 || name.Length > 100)
                throw new ArgumentException("El nombre del inquilino debe tener entre 2 y 100 caracteres", nameof(name));
            
            if (identifier.Length < 2 || identifier.Length > 50)
                throw new ArgumentException("El identificador del inquilino debe tener entre 2 y 50 caracteres", nameof(identifier));
            
            ValidateIdentifierFormat(identifier);
            
            TenantId = Guid.NewGuid();
            Name = name;
            Identifier = identifier;
            IsActive = true;
            CreatedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Identificador único del inquilino.
        /// </summary>
        [Key]
        public Guid TenantId { get; private set; }
        
        /// <summary>
        /// Nombre del inquilino, visible para administradores.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; private set; }
        
        /// <summary>
        /// Identificador único utilizado para acceso (ej. subdominio).
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "El identificador solo puede contener letras minúsculas, números y guiones.")]
        public string Identifier { get; private set; }
        
        /// <summary>
        /// Indica si el inquilino está activo y puede ser accedido.
        /// </summary>
        public bool IsActive { get; private set; }
        
        /// <summary>
        /// Fecha de creación del inquilino.
        /// </summary>
        public DateTime CreatedDate { get; private set; }

        /// <summary>
        /// Fecha de la última modificación del inquilino.
        /// </summary>
        public DateTime? LastModifiedDate { get; private set; }

        /// <summary>
        /// Actualiza el nombre del inquilino.
        /// </summary>
        /// <param name="name">Nuevo nombre.</param>
        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre del inquilino no puede estar vacío", nameof(name));
            
            if (name.Length < 2 || name.Length > 100)
                throw new ArgumentException("El nombre del inquilino debe tener entre 2 y 100 caracteres", nameof(name));
            
            Name = name;
            LastModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Actualiza el identificador del inquilino.
        /// </summary>
        /// <param name="identifier">Nuevo identificador.</param>
        public void UpdateIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("El identificador del inquilino no puede estar vacío", nameof(identifier));
            
            if (identifier.Length < 2 || identifier.Length > 50)
                throw new ArgumentException("El identificador del inquilino debe tener entre 2 y 50 caracteres", nameof(identifier));
            
            ValidateIdentifierFormat(identifier);
            
            Identifier = identifier;
            LastModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Activa el inquilino.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
            LastModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Desactiva el inquilino.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            LastModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Valida que el formato del identificador cumpla con los requisitos.
        /// </summary>
        /// <param name="identifier">Identificador a validar.</param>
        /// <exception cref="ArgumentException">Si el formato no es válido.</exception>
        private void ValidateIdentifierFormat(string identifier)
        {
            // El identificador debe comenzar con una letra, contener solo letras minúsculas, números y guiones, 
            // y no puede terminar con guion ni contener guiones consecutivos
            if (!Regex.IsMatch(identifier, @"^[a-z][a-z0-9\-]*[a-z0-9]$") || identifier.Contains("--"))
                throw new ArgumentException("El identificador debe comenzar con una letra minúscula, terminar con letra o número, y no puede contener guiones consecutivos", nameof(identifier));
        }

        /// <summary>
        /// Verifica si este inquilino puede ser accedido por los usuarios.
        /// </summary>
        /// <returns>True si el inquilino está activo y puede ser accedido, false de lo contrario.</returns>
        public bool CanBeAccessed()
        {
            return IsActive;
        }

        /// <summary>
        /// Verifica si este inquilino puede ser eliminado.
        /// En una implementación real, esto podría verificar la existencia de datos relacionados.
        /// </summary>
        /// <param name="hasUsers">Indica si el inquilino tiene usuarios.</param>
        /// <param name="hasSections">Indica si el inquilino tiene secciones definidas.</param>
        /// <returns>True si el inquilino puede ser eliminado, false de lo contrario.</returns>
        public bool CanBeDeleted(bool hasUsers, bool hasSections)
        {
            // Un inquilino no debería ser eliminado si tiene usuarios o secciones de datos
            return !hasUsers && !hasSections;
        }
    }
} 