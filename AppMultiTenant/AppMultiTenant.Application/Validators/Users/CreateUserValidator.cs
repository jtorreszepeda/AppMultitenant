using FluentValidation;
using AppMultiTenant.Application.Validators.Base;
using System;

namespace AppMultiTenant.Application.Validators.Users
{
    /// <summary>
    /// Validador para la creación de usuarios
    /// </summary>
    public class CreateUserValidator : AbstractValidator<(string UserName, string Email, string Password, string FullName, Guid TenantId)>
    {
        /// <summary>
        /// Constructor con las reglas de validación para crear usuarios
        /// </summary>
        public CreateUserValidator()
        {
            RuleFor(x => x.UserName).ValidName();
            RuleFor(x => x.Email).ValidEmail();
            RuleFor(x => x.Password).ValidPassword();
            RuleFor(x => x.FullName).ValidName();
            RuleFor(x => x.TenantId).ValidGuid();
        }
    }
} 