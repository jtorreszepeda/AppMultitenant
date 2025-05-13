using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AppMultiTenant.Domain.Entities
{
    /// <summary>
    /// Representa un permiso en el sistema.
    /// Los permisos definen las acciones específicas que los usuarios pueden realizar en la aplicación.
    /// </summary>
    public class Permission
    {
        // Constantes para permisos de sistema
        public static class SystemPermissions
        {
            public const string CreateUser = "CanCreateUser";
            public const string EditUser = "CanEditUser";
            public const string DeleteUser = "CanDeleteUser";
            public const string ViewUsers = "CanViewUsers";
            
            public const string CreateRole = "CanCreateRole";
            public const string EditRole = "CanEditRole";
            public const string DeleteRole = "CanDeleteRole";
            public const string ViewRoles = "CanViewRoles";
            
            public const string AssignRoles = "CanAssignRoles";
            public const string AssignPermissions = "CanAssignPermissions";
            
            public const string DefineSections = "CanDefineSections";
            public const string ViewAllSections = "CanViewAllSections";
        }

        /// <summary>
        /// Constructor privado para EF Core.
        /// </summary>
        private Permission() { }

        /// <summary>
        /// Constructor para crear un nuevo permiso.
        /// </summary>
        /// <param name="name">Nombre único del permiso.</param>
        /// <param name="description">Descripción detallada del permiso.</param>
        /// <param name="isSystemPermission">Indica si es un permiso del sistema (no puede modificarse).</param>
        public Permission(string name, string description, bool isSystemPermission = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre del permiso no puede estar vacío", nameof(name));
            
            ValidatePermissionName(name);
            
            PermissionId = Guid.NewGuid();
            Name = name;
            Description = description ?? string.Empty;
            IsSystemPermission = isSystemPermission;
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
        /// Indica si este permiso es un permiso del sistema y no debe modificarse.
        /// </summary>
        public bool IsSystemPermission { get; private set; }
        
        /// <summary>
        /// Fecha de creación del permiso.
        /// </summary>
        public DateTime CreatedDate { get; private set; }

        /// <summary>
        /// Fecha de última modificación del permiso.
        /// </summary>
        public DateTime? LastModifiedDate { get; private set; }

        /// <summary>
        /// Actualiza el nombre del permiso.
        /// </summary>
        /// <param name="name">Nuevo nombre.</param>
        public void UpdateName(string name)
        {
            if (IsSystemPermission)
                throw new InvalidOperationException("No se puede modificar un permiso del sistema");
                
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre del permiso no puede estar vacío", nameof(name));
            
            ValidatePermissionName(name);
            
            Name = name;
            LastModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Actualiza la descripción del permiso.
        /// </summary>
        /// <param name="description">Nueva descripción.</param>
        public void UpdateDescription(string description)
        {
            if (IsSystemPermission)
                throw new InvalidOperationException("No se puede modificar un permiso del sistema");
                
            Description = description ?? string.Empty;
            LastModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Valida que el formato del nombre del permiso sea correcto.
        /// </summary>
        /// <param name="name">Nombre del permiso a validar.</param>
        /// <exception cref="ArgumentException">Si el nombre no cumple con los requisitos.</exception>
        private void ValidatePermissionName(string name)
        {
            if (name.Length < 3 || name.Length > 100)
                throw new ArgumentException("El nombre del permiso debe tener entre 3 y 100 caracteres", nameof(name));
            
            // Los permisos deben seguir la convención "CanXxxYyy" 
            if (!Regex.IsMatch(name, @"^Can[A-Z][a-zA-Z0-9]*$") && !name.StartsWith("Can"))
                throw new ArgumentException("El nombre del permiso debe seguir el formato 'CanXxxYyy'", nameof(name));
        }

        /// <summary>
        /// Crea un conjunto de permisos para una sección específica.
        /// </summary>
        /// <param name="sectionName">Nombre de la sección.</param>
        /// <returns>Arreglo de permisos para la sección.</returns>
        public static Permission[] CreateSectionPermissions(string sectionName)
        {
            // Validar que el nombre de la sección sea válido
            if (string.IsNullOrWhiteSpace(sectionName))
                throw new ArgumentException("El nombre de la sección no puede estar vacío", nameof(sectionName));
            
            // Normalizar el nombre de la sección (primera letra en mayúscula, eliminar espacios)
            string normalizedName = sectionName.Trim();
            normalizedName = char.ToUpperInvariant(normalizedName[0]) + normalizedName.Substring(1);
            normalizedName = normalizedName.Replace(" ", "");
            
            return new[] 
            {
                new Permission($"CanCreateDataInSection{normalizedName}", 
                    $"Permite crear datos en la sección {sectionName}", true),
                
                new Permission($"CanReadDataInSection{normalizedName}", 
                    $"Permite ver datos en la sección {sectionName}", true),
                
                new Permission($"CanUpdateDataInSection{normalizedName}", 
                    $"Permite modificar datos en la sección {sectionName}", true),
                
                new Permission($"CanDeleteDataInSection{normalizedName}", 
                    $"Permite eliminar datos en la sección {sectionName}", true)
            };
        }

        /// <summary>
        /// Determina si este permiso puede ser eliminado.
        /// </summary>
        /// <param name="isAssignedToRoles">Indica si el permiso está asignado a roles.</param>
        /// <returns>True si el permiso puede ser eliminado, false de lo contrario.</returns>
        public bool CanBeDeleted(bool isAssignedToRoles)
        {
            // No se pueden eliminar permisos del sistema o permisos asignados a roles
            return !IsSystemPermission && !isAssignedToRoles;
        }
    }
} 