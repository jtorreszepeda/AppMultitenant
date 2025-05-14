using FluentValidation;
using AppMultiTenant.Application.Validators.Base;
using System;

namespace AppMultiTenant.Application.Validators.Sections
{
    /// <summary>
    /// Validadores para operaciones con secciones de la aplicación
    /// </summary>
    public class SectionValidator
    {
        /// <summary>
        /// Validador para la creación de definiciones de secciones
        /// </summary>
        public class CreateSectionDefinitionValidator : AbstractValidator<(string Name, string Description, string IconName, int DisplayOrder, Guid TenantId)>
        {
            /// <summary>
            /// Constructor con las reglas de validación para crear definiciones de secciones
            /// </summary>
            public CreateSectionDefinitionValidator()
            {
                RuleFor(x => x.Name).ValidName();
                RuleFor(x => x.Description).ValidDescription();
                RuleFor(x => x.IconName)
                    .NotEmpty().WithMessage("El nombre del icono no puede estar vacío.")
                    .MaximumLength(50).WithMessage("El nombre del icono no puede tener más de 50 caracteres.");
                RuleFor(x => x.DisplayOrder)
                    .GreaterThanOrEqualTo(0).WithMessage("El orden de visualización debe ser mayor o igual a 0.");
                RuleFor(x => x.TenantId).ValidGuid();
            }
        }

        /// <summary>
        /// Validador para la actualización de definiciones de secciones
        /// </summary>
        public class UpdateSectionDefinitionValidator : AbstractValidator<(Guid SectionId, string Name, string Description, string IconName, int DisplayOrder, Guid TenantId)>
        {
            /// <summary>
            /// Constructor con las reglas de validación para actualizar definiciones de secciones
            /// </summary>
            public UpdateSectionDefinitionValidator()
            {
                RuleFor(x => x.SectionId).ValidGuid();
                RuleFor(x => x.Name).ValidName();
                RuleFor(x => x.Description).ValidDescription();
                RuleFor(x => x.IconName)
                    .NotEmpty().WithMessage("El nombre del icono no puede estar vacío.")
                    .MaximumLength(50).WithMessage("El nombre del icono no puede tener más de 50 caracteres.");
                RuleFor(x => x.DisplayOrder)
                    .GreaterThanOrEqualTo(0).WithMessage("El orden de visualización debe ser mayor o igual a 0.");
                RuleFor(x => x.TenantId).ValidGuid();
            }
        }

        /// <summary>
        /// Validador para la actualización del orden de visualización de secciones
        /// </summary>
        public class UpdateSectionOrderValidator : AbstractValidator<(Guid SectionId, int DisplayOrder, Guid TenantId)>
        {
            /// <summary>
            /// Constructor con las reglas de validación para actualizar el orden de visualización
            /// </summary>
            public UpdateSectionOrderValidator()
            {
                RuleFor(x => x.SectionId).ValidGuid();
                RuleFor(x => x.DisplayOrder)
                    .GreaterThanOrEqualTo(0).WithMessage("El orden de visualización debe ser mayor o igual a 0.");
                RuleFor(x => x.TenantId).ValidGuid();
            }
        }
    }
} 