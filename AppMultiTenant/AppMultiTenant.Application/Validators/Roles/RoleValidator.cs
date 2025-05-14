using FluentValidation;
using AppMultiTenant.Application.Validators.Base;
using System;
using System.Collections.Generic;

namespace AppMultiTenant.Application.Validators.Roles
{
    /// <summary>
    /// Validadores para operaciones con roles
    /// </summary>
    public class RoleValidator
    {
        /// <summary>
        /// Validador para la creación de roles
        /// </summary>
        public class CreateRoleValidator : AbstractValidator<(string Name, string Description, Guid TenantId)>
        {
            /// <summary>
            /// Constructor con las reglas de validación para crear roles
            /// </summary>
            public CreateRoleValidator()
            {
                RuleFor(x => x.Name).ValidName();
                RuleFor(x => x.Description).ValidDescription();
                RuleFor(x => x.TenantId).ValidGuid();
            }
        }

        /// <summary>
        /// Validador para la actualización de roles
        /// </summary>
        public class UpdateRoleValidator : AbstractValidator<(Guid RoleId, string Name, string Description, Guid TenantId)>
        {
            /// <summary>
            /// Constructor con las reglas de validación para actualizar roles
            /// </summary>
            public UpdateRoleValidator()
            {
                RuleFor(x => x.RoleId).ValidGuid();
                RuleFor(x => x.Name).ValidName();
                RuleFor(x => x.Description).ValidDescription();
                RuleFor(x => x.TenantId).ValidGuid();
            }
        }

        /// <summary>
        /// Validador para asignar permisos a un rol
        /// </summary>
        public class AssignPermissionsValidator : AbstractValidator<(Guid RoleId, IEnumerable<string> PermissionNames, Guid TenantId)>
        {
            /// <summary>
            /// Constructor con las reglas de validación para asignar permisos
            /// </summary>
            public AssignPermissionsValidator()
            {
                RuleFor(x => x.RoleId).ValidGuid();
                RuleFor(x => x.PermissionNames).NotNull().WithMessage("La lista de permisos no puede ser nula.");
                RuleForEach(x => x.PermissionNames).NotEmpty().WithMessage("Los nombres de los permisos no pueden estar vacíos.");
                RuleFor(x => x.TenantId).ValidGuid();
            }
        }

        /// <summary>
        /// Validador para asignar roles a un usuario
        /// </summary>
        public class AssignRolesToUserValidator : AbstractValidator<(Guid UserId, IEnumerable<Guid> RoleIds, Guid TenantId)>
        {
            /// <summary>
            /// Constructor con las reglas de validación para asignar roles a un usuario
            /// </summary>
            public AssignRolesToUserValidator()
            {
                RuleFor(x => x.UserId).ValidGuid();
                RuleFor(x => x.RoleIds).NotNull().WithMessage("La lista de IDs de roles no puede ser nula.");
                RuleForEach(x => x.RoleIds).ValidGuid();
                RuleFor(x => x.TenantId).ValidGuid();
            }
        }
    }
} 