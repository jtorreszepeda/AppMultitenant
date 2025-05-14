using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppMultiTenant.Domain.Entities
{
    /// <summary>
    /// Entidad de unión que representa la relación muchos a muchos entre Roles y Permisos.
    /// Implementa ITenantEntity para mantener el filtrado automático por inquilino.
    /// </summary>
    public class RolePermission : ITenantEntity
    {
        /// <summary>
        /// Constructor privado para EF Core.
        /// </summary>
        private RolePermission() { }

        /// <summary>
        /// Constructor para crear una nueva relación entre rol y permiso.
        /// </summary>
        /// <param name="roleId">ID del rol.</param>
        /// <param name="permissionId">ID del permiso.</param>
        /// <param name="tenantId">ID del inquilino al que pertenece esta relación.</param>
        public RolePermission(Guid roleId, Guid permissionId, Guid tenantId)
        {
            ValidateRoleId(roleId);
            ValidatePermissionId(permissionId);
            ValidateTenantId(tenantId);
            
            Id = roleId;
            PermissionId = permissionId;
            TenantId = tenantId;
            CreatedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// ID del rol en esta relación.
        /// </summary>
        [Required]
        public Guid Id { get; private set; }

        /// <summary>
        /// ID del permiso en esta relación.
        /// </summary>
        [Required]
        public Guid PermissionId { get; private set; }

        /// <summary>
        /// Identificador del inquilino al que pertenece esta relación.
        /// Esto permite el filtrado de permisos por inquilino.
        /// </summary>
        [Required]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Fecha de creación de la relación.
        /// </summary>
        public DateTime CreatedDate { get; private set; }

        /// <summary>
        /// Propiedad de navegación al rol.
        /// </summary>
        [ForeignKey(nameof(Id))]
        public virtual ApplicationRole Role { get; private set; }

        /// <summary>
        /// Propiedad de navegación al permiso.
        /// </summary>
        [ForeignKey(nameof(PermissionId))]
        public virtual Permission Permission { get; private set; }

        /// <summary>
        /// Valida que el RoleId sea válido.
        /// </summary>
        /// <param name="roleId">ID del rol a validar.</param>
        /// <exception cref="ArgumentException">Si el RoleId no es válido.</exception>
        private void ValidateRoleId(Guid roleId)
        {
            if (roleId == Guid.Empty)
                throw new ArgumentException("El ID del rol no puede estar vacío", nameof(roleId));
        }

        /// <summary>
        /// Valida que el PermissionId sea válido.
        /// </summary>
        /// <param name="permissionId">ID del permiso a validar.</param>
        /// <exception cref="ArgumentException">Si el PermissionId no es válido.</exception>
        private void ValidatePermissionId(Guid permissionId)
        {
            if (permissionId == Guid.Empty)
                throw new ArgumentException("El ID del permiso no puede estar vacío", nameof(permissionId));
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
        /// Verifica si esta asignación de permiso al rol es válida según las reglas de negocio.
        /// </summary>
        /// <param name="role">El rol al que se asigna el permiso.</param>
        /// <param name="permission">El permiso que se está asignando.</param>
        /// <returns>True si la asignación es válida, false de lo contrario.</returns>
        public static bool IsValidAssignment(ApplicationRole role, Permission permission)
        {
            if (role == null || permission == null)
                return false;

            // El rol y el permiso deben pertenecer al mismo inquilino (excepto permisos del sistema)
            // Los permisos de sistema no están asociados a un inquilino específico
            if (role.TenantId != Guid.Empty && !permission.IsSystemPermission)
            {
                // Aquí se implementarían reglas adicionales si hubiera restricciones específicas
                // sobre qué permisos pueden asignarse a qué roles
                
                // Por ejemplo, podríamos tener permisos avanzados que solo el rol de administrador puede tener
                return true;
            }

            // Por defecto, permitir la asignación si el permiso es de sistema
            return permission.IsSystemPermission;
        }
    }
} 