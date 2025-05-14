using System;
using FluentValidation;

namespace AppMultiTenant.Application.Validators.Base
{
    /// <summary>
    /// Clase base para validadores con reglas comunes
    /// </summary>
    public static class ValidationBase
    {
        /// <summary>
        /// Reglas de validación para nombres (1-100 caracteres)
        /// </summary>
        public static IRuleBuilderOptions<T, string> ValidName<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("El nombre no puede estar vacío.")
                .Length(1, 100).WithMessage("El nombre debe tener entre 1 y 100 caracteres.");
        }

        /// <summary>
        /// Reglas de validación para direcciones de correo electrónico
        /// </summary>
        public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("El correo electrónico no puede estar vacío.")
                .EmailAddress().WithMessage("El formato del correo electrónico no es válido.")
                .MaximumLength(256).WithMessage("El correo electrónico no puede tener más de 256 caracteres.");
        }

        /// <summary>
        /// Reglas de validación para contraseñas
        /// </summary>
        public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("La contraseña no puede estar vacía.")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
                .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula.")
                .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula.")
                .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número.")
                .Matches("[^a-zA-Z0-9]").WithMessage("La contraseña debe contener al menos un carácter especial.");
        }

        /// <summary>
        /// Reglas de validación para identificadores GUID
        /// </summary>
        public static IRuleBuilderOptions<T, Guid> ValidGuid<T>(this IRuleBuilder<T, Guid> ruleBuilder)
        {
            return ruleBuilder
                .NotEqual(Guid.Empty).WithMessage("El identificador no puede estar vacío.");
        }

        /// <summary>
        /// Reglas de validación para texto largo (descripción, etc.)
        /// </summary>
        public static IRuleBuilderOptions<T, string> ValidDescription<T>(this IRuleBuilder<T, string> ruleBuilder, bool required = false)
        {
            var rule = ruleBuilder.MaximumLength(1000).WithMessage("La descripción no puede tener más de 1000 caracteres.");
            
            if (required)
            {
                rule = rule.NotEmpty().WithMessage("La descripción no puede estar vacía.");
            }
            
            return rule;
        }
    }
} 