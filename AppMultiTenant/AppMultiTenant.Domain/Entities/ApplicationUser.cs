using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AppMultiTenant.Domain.Entities
{
    /// <summary>
    /// Representa un usuario en el sistema multi-inquilino.
    /// Extiende IdentityUser<Guid> e implementa ITenantEntity para permitir
    /// el filtrado automático por inquilino.
    /// </summary>
    public class ApplicationUser : IdentityUser<Guid>, ITenantEntity
    {
        /// <summary>
        /// Constructor privado para EF Core.
        /// </summary>
        private ApplicationUser() { }

        /// <summary>
        /// Constructor para crear un nuevo usuario asociado a un inquilino.
        /// </summary>
        /// <param name="userName">Nombre de usuario único.</param>
        /// <param name="email">Correo electrónico del usuario.</param>
        /// <param name="tenantId">ID del inquilino al que pertenece.</param>
        public ApplicationUser(string userName, string email, Guid tenantId)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("El nombre de usuario no puede estar vacío", nameof(userName));
            
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El correo electrónico no puede estar vacío", nameof(email));
            
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));
            
            Id = Guid.NewGuid();
            UserName = userName;
            Email = email;
            TenantId = tenantId;
            IsActive = true;
            CreatedDate = DateTime.UtcNow;
            EmailConfirmed = false;
            PhoneNumberConfirmed = false;
            TwoFactorEnabled = false;
            LockoutEnabled = true;
            AccessFailedCount = 0;
        }

        /// <summary>
        /// Identificador del inquilino al que pertenece este usuario.
        /// </summary>
        [Required]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Nombre completo del usuario.
        /// </summary>
        [StringLength(100)]
        public string FullName { get; private set; }

        /// <summary>
        /// Indica si el usuario está activo y puede iniciar sesión.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Fecha de creación del usuario.
        /// </summary>
        public DateTime CreatedDate { get; private set; }

        /// <summary>
        /// Última fecha de inicio de sesión del usuario.
        /// </summary>
        public DateTime? LastLoginDate { get; private set; }

        /// <summary>
        /// Actualiza el nombre completo del usuario.
        /// </summary>
        /// <param name="fullName">Nuevo nombre completo.</param>
        public void UpdateFullName(string fullName)
        {
            if (!string.IsNullOrWhiteSpace(fullName) && fullName.Length <= 100)
            {
                FullName = fullName;
            }
            else
            {
                throw new ArgumentException("El nombre completo debe tener entre 1 y 100 caracteres", nameof(fullName));
            }
        }

        /// <summary>
        /// Actualiza el correo electrónico del usuario.
        /// </summary>
        /// <param name="email">Nuevo correo electrónico.</param>
        public void UpdateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El correo electrónico no puede estar vacío", nameof(email));
            
            Email = email;
            EmailConfirmed = false; // Requiere nueva confirmación
        }

        /// <summary>
        /// Activa el usuario.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Desactiva el usuario.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// Registra un inicio de sesión exitoso.
        /// </summary>
        public void RecordLogin()
        {
            LastLoginDate = DateTime.UtcNow;
        }
    }
} 