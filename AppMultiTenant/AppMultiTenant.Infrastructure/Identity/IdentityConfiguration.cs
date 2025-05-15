using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AppMultiTenant.Domain.Entities;
using AppMultiTenant.Infrastructure.Persistence;
using System;

namespace AppMultiTenant.Infrastructure.Identity
{
    /// <summary>
    /// Contiene métodos de extensión para configurar servicios de Identity
    /// adaptados para soportar multi-tenancy
    /// </summary>
    public static class IdentityConfiguration
    {
        /// <summary>
        /// Agrega y configura ASP.NET Core Identity para la aplicación multi-inquilino
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <returns>Colección de servicios actualizada</returns>
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Registrar los stores personalizados que manejan el TenantId
            services.AddScoped<IUserStore<ApplicationUser>, MultiTenantUserStore>();
            services.AddScoped<IRoleStore<ApplicationRole>, MultiTenantRoleStore>();
            
            // Configurar Identity con nuestras entidades personalizadas
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Configuración de contraseñas
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                // Configuración de bloqueo de cuenta
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // Configuración de usuario
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                // Configuración de inicio de sesión
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            // Nota: No usamos .AddEntityFrameworkStores<AppDbContext>() aquí
            // porque ya registramos nuestros stores personalizados arriba
            .AddDefaultTokenProviders();

            // Configuración adicional para tokens de restablecimiento de contraseña, confirmación de correo, etc.
            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(3);
            });

            return services;
        }
    }
} 