using System;
using AppMultiTenant.Application.Interfaces.Services;
using AppMultiTenant.Application.Services;
using AppMultiTenant.Application.Validators.Auth;
using AppMultiTenant.Application.Validators.Roles;
using AppMultiTenant.Application.Validators.Sections;
using AppMultiTenant.Application.Validators.Tenants;
using AppMultiTenant.Application.Validators.Users;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AppMultiTenant.Application.Configuration
{
    /// <summary>
    /// Extensiones para registrar los validadores y el servicio de validación
    /// </summary>
    public static class ValidatorRegistration
    {
        /// <summary>
        /// Registra el servicio de validación y todos los validadores
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <returns>La colección de servicios con los validadores registrados</returns>
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            // Registrar el servicio de validación
            services.AddScoped<IValidationService, ValidationService>();

            // Registrar validadores manualmente
            // Validadores de autenticación
            services.AddScoped<IValidator<(string Email, string Password)>, LoginValidator>();
            services.AddScoped<IValidator<(string UserName, string Email, string Password, string FullName, Guid TenantId)>, RegisterValidator>();

            // Validadores de usuarios
            services.AddScoped<IValidator<(string UserName, string Email, string Password, string FullName, Guid TenantId)>, CreateUserValidator>();
            services.AddScoped<IValidator<(Guid UserId, string FullName, Guid TenantId)>, UpdateUserValidator.UpdateFullNameValidator>();
            services.AddScoped<IValidator<(Guid UserId, string Email, Guid TenantId)>, UpdateUserValidator.UpdateEmailValidator>();
            services.AddScoped<IValidator<(Guid UserId, string UserName, Guid TenantId)>, UpdateUserValidator.UpdateUserNameValidator>();
            services.AddScoped<IValidator<(Guid UserId, string NewPassword, string CurrentPassword, Guid TenantId)>, UpdateUserValidator.ChangePasswordValidator>();
            
            // Validadores de roles
            services.AddScoped<IValidator<(string Name, string Description, Guid TenantId)>, RoleValidator.CreateRoleValidator>();
            services.AddScoped<IValidator<(Guid RoleId, string Name, string Description, Guid TenantId)>, RoleValidator.UpdateRoleValidator>();
            services.AddScoped<IValidator<(Guid RoleId, IEnumerable<string> PermissionNames, Guid TenantId)>, RoleValidator.AssignPermissionsValidator>();
            services.AddScoped<IValidator<(Guid UserId, IEnumerable<Guid> RoleIds, Guid TenantId)>, RoleValidator.AssignRolesToUserValidator>();
            
            // Validadores de inquilinos
            services.AddScoped<IValidator<(string Name, string Domain, string AdminEmail, string AdminPassword, string AdminFullName)>, TenantValidator.CreateTenantValidator>();
            services.AddScoped<IValidator<(Guid TenantId, string Name, string Domain)>, TenantValidator.UpdateTenantValidator>();
            
            // Validadores de secciones
            services.AddScoped<IValidator<(string Name, string Description, string IconName, int DisplayOrder, Guid TenantId)>, SectionValidator.CreateSectionDefinitionValidator>();
            services.AddScoped<IValidator<(Guid SectionId, string Name, string Description, string IconName, int DisplayOrder, Guid TenantId)>, SectionValidator.UpdateSectionDefinitionValidator>();
            services.AddScoped<IValidator<(Guid SectionId, int DisplayOrder, Guid TenantId)>, SectionValidator.UpdateSectionOrderValidator>();

            return services;
        }
    }
} 