using FluentValidation;
using AppMultiTenant.Application.Validators.Base;
using System;

namespace AppMultiTenant.Application.Validators.Auth
{
    /// <summary>
    /// Validador para operaciones de registro de usuarios
    /// </summary>
    public class RegisterValidator : AbstractValidator<(string UserName, string Email, string Password, string FullName, Guid TenantId)>
    {
        /// <summary>
        /// Constructor con las reglas de validación para el registro
        /// </summary>
        public RegisterValidator()
        {
            RuleFor(x => x.UserName).ValidName();
            RuleFor(x => x.Email).ValidEmail();
            RuleFor(x => x.Password).ValidPassword();
            RuleFor(x => x.FullName).ValidName();
            RuleFor(x => x.TenantId).ValidGuid();
        }
    }
} 