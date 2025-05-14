using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.IO;

namespace AppMultiTenant.Infrastructure.Persistence.Configuration
{
    /// <summary>
    /// Factory para crear instancias de AppDbContext durante el diseño (migraciones)
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        /// <summary>
        /// Crea una nueva instancia de AppDbContext para usar durante las migraciones de EF Core
        /// </summary>
        /// <param name="args">Argumentos de línea de comandos</param>
        /// <returns>Una instancia de AppDbContext configurada para migraciones</returns>
        public AppDbContext CreateDbContext(string[] args)
        {
            // Obtener la configuración desde appsettings.json
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Crear las opciones del contexto con PostgreSQL
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

            // Crear el contexto con un TenantId nulo (para operaciones a nivel de sistema)
            return new AppDbContext(optionsBuilder.Options, (Guid?)null);
        }
    }
} 