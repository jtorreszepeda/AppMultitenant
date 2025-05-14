using FluentValidation;
using AppMultiTenant.Application.Validators.Base;
using System;

namespace AppMultiTenant.Application.Validators.Users
{
    /// <summary>
    /// Validador para la actualización de datos de usuario
    /// </summary>
    public class UpdateUserValidator
    {
        /// <summary>
        /// Validador para la actualización del nombre completo
        /// </summary>
        public class UpdateFullNameValidator : AbstractValidator<(Guid UserId, string FullName, Guid TenantId)>
        {
            /// <summary>
            /// Constructor con las reglas de validación para actualizar nombre completo
            /// </summary>
            public UpdateFullNameValidator()
            {
                RuleFor(x => x.UserId).ValidGuid();
                RuleFor(x => x.FullName).ValidName();
                RuleFor(x => x.TenantId).ValidGuid();
            }
        }

        /// <summary>
        /// Validador para la actualización del email
        /// </summary>
        public class UpdateEmailValidator : AbstractValidator<(Guid UserId, string Email, Guid TenantId)>
        {
            /// <summary>
            /// Constructor con las reglas de validación para actualizar email
            /// </summary>
            public UpdateEmailValidator()
            {
                RuleFor(x => x.UserId).ValidGuid();
                RuleFor(x => x.Email).ValidEmail();
                RuleFor(x => x.TenantId).ValidGuid();
            }
        }

        /// <summary>
        /// Validador para la actualización del nombre de usuario
        /// </summary>
        public class UpdateUserNameValidator : AbstractValidator<(Guid UserId, string UserName, Guid TenantId)>
        {
            /// <summary>
            /// Constructor con las reglas de validación para actualizar nombre de usuario
            /// </summary>
            public UpdateUserNameValidator()
            {
                RuleFor(x => x.UserId).ValidGuid();
                RuleFor(x => x.UserName).ValidName();
                RuleFor(x => x.TenantId).ValidGuid();
            }
        }

        /// <summary>
        /// Validador para el cambio de contraseña
        /// </summary>
        public class ChangePasswordValidator : AbstractValidator<(Guid UserId, string NewPassword, string CurrentPassword, Guid TenantId)>
        {
            /// <summary>
            /// Constructor con las reglas de validación para cambiar contraseña
            /// </summary>
            public ChangePasswordValidator()
            {
                RuleFor(x => x.UserId).ValidGuid();
                RuleFor(x => x.NewPassword).ValidPassword();
                // CurrentPassword puede ser nulo cuando lo hace un administrador
                When(x => !string.IsNullOrEmpty(x.CurrentPassword), () =>
                {
                    RuleFor(x => x.CurrentPassword).NotEmpty();
                });
                RuleFor(x => x.TenantId).ValidGuid();
            }
        }
    }
} 