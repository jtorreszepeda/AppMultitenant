using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.Tasks;
using AppMultiTenant.Application.Configuration;

namespace AppMultiTenant.Infrastructure.Identity
{
    /// <summary>
    /// Contiene métodos de extensión para configurar la autenticación JWT
    /// </summary>
    public static class JwtConfiguration
    {
        /// <summary>
        /// Configura la autenticación JWT para la aplicación
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <returns>Colección de servicios actualizada</returns>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettingsSection = configuration.GetSection("JwtSettings");
            var jwtSettings = jwtSettingsSection.Get<JwtSettings>() ?? new JwtSettings();

            if (string.IsNullOrEmpty(jwtSettings.SecretKey))
            {
                throw new InvalidOperationException("JwtSettings configuration is missing or incomplete. Make sure you have valid SecretKey configured.");
            }

            // Configurar autenticación JWT
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                        ClockSkew = TimeSpan.Zero // Elimina el margen de tolerancia de 5 minutos por defecto
                    };

                    // Eventos de autenticación JwtBearer
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // Si es necesario, aquí podemos extraer el token JWT de otras ubicaciones,
                            // como cookies o query strings para websockets
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            // Cuando se valida un token satisfactoriamente, podemos realizar operaciones adicionales
                            // como cargar datos de usuario desde la base de datos o verificar que el usuario aún existe
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            // Cuando falla la autenticación, podemos realizar acciones como logging
                            // En desarrollo, podemos enviar mensajes detallados
                            if (context.Exception is SecurityTokenExpiredException)
                            {
                                context.Response.Headers.Add("Token-Expired", "true");
                            }
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            // Si queremos anular el comportamiento predeterminado cuando se requiere autenticación
                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }
    }
} 