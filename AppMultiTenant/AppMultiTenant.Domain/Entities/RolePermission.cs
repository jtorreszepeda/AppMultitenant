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
            if (roleId == Guid.Empty)
                throw new ArgumentException("El ID del rol no puede estar vacío", nameof(roleId));
            
            if (permissionId == Guid.Empty)
                throw new ArgumentException("El ID del permiso no puede estar vacío", nameof(permissionId));
            
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));
            
            RoleId = roleId;
            PermissionId = permissionId;
            TenantId = tenantId;
            CreatedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// ID del rol en esta relación.
        /// </summary>
        [Required]
        public Guid RoleId { get; private set; }

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
        [ForeignKey(nameof(RoleId))]
        public virtual ApplicationRole Role { get; private set; }

        /// <summary>
        /// Propiedad de navegación al permiso.
        /// </summary>
        [ForeignKey(nameof(PermissionId))]
        public virtual Permission Permission { get; private set; }
    }
} 