using FluentValidation;
using AppMultiTenant.Application.Validators.Base;
using System;

namespace AppMultiTenant.Application.Validators.Tenants
{
    /// <summary>
    /// Validadores para operaciones con inquilinos
    /// </summary>
    public class TenantValidator
    {
        /// <summary>
        /// Validador para la creación de inquilinos
        /// </summary>
        public class CreateTenantValidator : AbstractValidator<(string Name, string Domain, string AdminEmail, string AdminPassword, string AdminFullName)>
        {
            /// <summary>
            /// Constructor con las reglas de validación para crear inquilinos
            /// </summary>
            public CreateTenantValidator()
            {
                RuleFor(x => x.Name).ValidName();
                RuleFor(x => x.Domain)
                    .NotEmpty().WithMessage("El dominio no puede estar vacío.")
                    .MaximumLength(100).WithMessage("El dominio no puede tener más de 100 caracteres.")
                    .Matches(@"^[a-zA-Z0-9][a-zA-Z0-9-]*[a-zA-Z0-9]$").WithMessage("El dominio solo puede contener letras, números y guiones. No puede comenzar ni terminar con guión.");
                RuleFor(x => x.AdminEmail).ValidEmail();
                RuleFor(x => x.AdminPassword).ValidPassword();
                RuleFor(x => x.AdminFullName).ValidName();
            }
        }

        /// <summary>
        /// Validador para la actualización de inquilinos
        /// </summary>
        public class UpdateTenantValidator : AbstractValidator<(Guid TenantId, string Name, string Domain)>
        {
            /// <summary>
            /// Constructor con las reglas de validación para actualizar inquilinos
            /// </summary>
            public UpdateTenantValidator()
            {
                RuleFor(x => x.TenantId).ValidGuid();
                RuleFor(x => x.Name).ValidName();
                RuleFor(x => x.Domain)
                    .NotEmpty().WithMessage("El dominio no puede estar vacío.")
                    .MaximumLength(100).WithMessage("El dominio no puede tener más de 100 caracteres.")
                    .Matches(@"^[a-zA-Z0-9][a-zA-Z0-9-]*[a-zA-Z0-9]$").WithMessage("El dominio solo puede contener letras, números y guiones. No puede comenzar ni terminar con guión.");
            }
        }
    }
} 