using FluentValidation;
using AppMultiTenant.Application.Validators.Base;

namespace AppMultiTenant.Application.Validators.Auth
{
    /// <summary>
    /// Validador para operaciones de login
    /// </summary>
    public class LoginValidator : AbstractValidator<(string Email, string Password)>
    {
        /// <summary>
        /// Constructor con las reglas de validación para el login
        /// </summary>
        public LoginValidator()
        {
            RuleFor(x => x.Email).ValidEmail();
            RuleFor(x => x.Password).ValidPassword();
        }
    }
} 