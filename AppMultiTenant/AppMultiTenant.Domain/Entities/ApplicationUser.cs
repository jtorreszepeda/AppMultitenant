using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
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
        // Expresión regular para validar email
        private static readonly Regex EmailRegex = new Regex(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
            
            ValidateEmail(email);
            ValidateTenantId(tenantId);
            
            Id = Guid.NewGuid();
            UserName = userName;
            NormalizedUserName = userName.ToUpperInvariant();
            Email = email;
            NormalizedEmail = email.ToUpperInvariant();
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
        /// Última fecha de modificación del usuario.
        /// </summary>
        public DateTime? LastModifiedDate { get; private set; }

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
                LastModifiedDate = DateTime.UtcNow;
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
            
            ValidateEmail(email);
            
            Email = email;
            NormalizedEmail = email.ToUpperInvariant();
            EmailConfirmed = false; // Requiere nueva confirmación
            LastModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Actualiza el nombre de usuario.
        /// </summary>
        /// <param name="userName">Nuevo nombre de usuario.</param>
        public void UpdateUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("El nombre de usuario no puede estar vacío", nameof(userName));
            
            UserName = userName;
            NormalizedUserName = userName.ToUpperInvariant();
            LastModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Activa el usuario.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
            LastModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Desactiva el usuario.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            LastModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Registra un inicio de sesión exitoso.
        /// </summary>
        public void RecordLogin()
        {
            LastLoginDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Valida que el formato del correo electrónico sea correcto.
        /// </summary>
        /// <param name="email">Correo electrónico a validar.</param>
        /// <exception cref="ArgumentException">Si el formato de correo electrónico no es válido.</exception>
        private void ValidateEmail(string email)
        {
            if (!EmailRegex.IsMatch(email))
                throw new ArgumentException("El formato del correo electrónico no es válido", nameof(email));
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
        /// Determina si el usuario puede iniciar sesión.
        /// </summary>
        /// <returns>True si el usuario puede iniciar sesión, false de lo contrario.</returns>
        public bool CanLogin()
        {
            return IsActive && !LockoutEnabled || (LockoutEnabled && (!LockoutEnd.HasValue || LockoutEnd.Value <= DateTimeOffset.UtcNow));
        }

        /// <summary>
        /// Determina si el usuario puede ser eliminado.
        /// </summary>
        /// <param name="isCurrentUser">Indica si es el usuario actual que realiza la operación.</param>
        /// <param name="isLastAdminOfTenant">Indica si es el último administrador del inquilino.</param>
        /// <returns>True si el usuario puede ser eliminado, false de lo contrario.</returns>
        public bool CanBeDeleted(bool isCurrentUser, bool isLastAdminOfTenant)
        {
            // No permitir eliminar el usuario actual o el último administrador
            return !isCurrentUser && !isLastAdminOfTenant;
        }
    }
} 