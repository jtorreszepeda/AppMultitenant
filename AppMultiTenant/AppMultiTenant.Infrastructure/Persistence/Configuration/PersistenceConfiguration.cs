using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AppMultiTenant.Application.Interfaces.Persistence;
using AppMultiTenant.Infrastructure.Persistence.Repositories;

namespace AppMultiTenant.Infrastructure.Persistence.Configuration
{
    /// <summary>
    /// Contiene métodos de extensión para configurar servicios de persistencia y Entity Framework Core
    /// </summary>
    public static class PersistenceConfiguration
    {
        /// <summary>
        /// Agrega los servicios de Entity Framework Core y configura el AppDbContext
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <returns>Colección de servicios actualizada</returns>
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Registrar AppDbContext con PostgreSQL y resolver ITenantResolverService para evitar ambigüedad de constructores
            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var tenantResolver = serviceProvider.GetRequiredService<AppMultiTenant.Application.Interfaces.Services.ITenantResolverService>();
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                );
            },
            ServiceLifetime.Scoped,
            ServiceLifetime.Scoped
            );

            // Registrar repositorios
            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IAppSectionDefinitionRepository, AppSectionDefinitionRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            return services;
        }
    }
} 