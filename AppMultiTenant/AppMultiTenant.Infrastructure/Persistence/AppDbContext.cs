using Microsoft.EntityFrameworkCore;
using AppMultiTenant.Domain.Entities;
using AppMultiTenant.Application.Interfaces.Services;

namespace AppMultiTenant.Infrastructure.Persistence
{
    /// <summary>
    /// Contexto de base de datos principal de la aplicación multi-inquilino.
    /// Implementa el enfoque de base de datos compartida con esquema compartido y discriminador TenantId.
    /// </summary>
    public class AppDbContext : DbContext
    {
        private readonly Guid? _currentTenantId;

        /// <summary>
        /// Constructor para ser usado con Inyección de Dependencias
        /// </summary>
        /// <param name="options">Opciones del DbContext</param>
        /// <param name="tenantService">Servicio para obtener el inquilino actual</param>
        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            ITenantResolverService tenantService) : base(options)
        {
            _currentTenantId = tenantService.GetCurrentTenantId();
        }

        /// <summary>
        /// Constructor para uso en pruebas o migración
        /// </summary>
        /// <param name="options">Opciones del DbContext</param>
        /// <param name="tenantId">ID del inquilino o null para el modo admin global</param>
        public AppDbContext(
            DbContextOptions<AppDbContext> options, 
            Guid? tenantId = null) : base(options)
        {
            _currentTenantId = tenantId;
        }

        /// <summary>
        /// Colección de inquilinos en el sistema
        /// </summary>
        public DbSet<Tenant> Tenants => Set<Tenant>();

        // Aquí se agregarán más DbSet para otras entidades del dominio
        // Ejemplos para futuras implementaciones:
        // public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
        // public DbSet<ApplicationRole> Roles => Set<ApplicationRole>();
        // public DbSet<Permission> Permissions => Set<Permission>();
        // public DbSet<AppSectionDefinition> SectionDefinitions => Set<AppSectionDefinition>();
        // public DbSet<FieldDefinition> FieldDefinitions => Set<FieldDefinition>();
        // public DbSet<SectionDataEntry> SectionDataEntries => Set<SectionDataEntry>();
        // public DbSet<FieldValue> FieldValues => Set<FieldValue>();

        /// <summary>
        /// Configura el modelo y aplica los Global Query Filters para el aislamiento de datos por inquilino
        /// </summary>
        /// <param name="modelBuilder">Builder para el modelo de EF Core</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración para la entidad Tenant
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.ToTable("Tenants");
                entity.HasKey(e => e.TenantId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Identifier).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Identifier).IsUnique();
            });

            // Aquí se incluirán las configuraciones para otras entidades multi-inquilino
            // cuando se implementen, aplicando los Global Query Filters
            
            // Ejemplo para ilustrar cómo se aplicarán los filtros a futuras entidades:
            /*
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                
                // Aplicar Global Query Filter para TenantId
                // Esto filtrará automáticamente los usuarios por el inquilino actual
                entity.HasQueryFilter(e => _currentTenantId == null || e.TenantId == _currentTenantId);
                
                // Otras configuraciones...
            });
            */
        }

        /// <summary>
        /// Sobrescribe SaveChanges para garantizar que todas las entidades nuevas
        /// con TenantId se asignen al inquilino actual
        /// </summary>
        public override int SaveChanges()
        {
            ApplyTenantFiltering();
            return base.SaveChanges();
        }

        /// <summary>
        /// Sobrescribe SaveChangesAsync para garantizar que todas las entidades nuevas
        /// con TenantId se asignen al inquilino actual
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyTenantFiltering();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Aplica el TenantId actual a las entidades nuevas que implementan ITenantEntity
        /// </summary>
        private void ApplyTenantFiltering()
        {
            // Si no hay un inquilino actual en el contexto, no aplicamos filtrado
            // Esto permite que el SuperAdmin opere a nivel global
            if (_currentTenantId == null)
                return;

            // Obtenemos todas las entidades en estado Added que implementan ITenantEntity
            var entities = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added && e.Entity is ITenantEntity)
                .Select(e => e.Entity as ITenantEntity);

            foreach (var entity in entities)
            {
                // Si la entidad ya tiene un TenantId asignado y no coincide con el actual,
                // lanzamos una excepción para evitar la asignación incorrecta de datos
                if (entity!.TenantId != default && entity.TenantId != _currentTenantId)
                {
                    throw new InvalidOperationException($"Intentando guardar una entidad con TenantId {entity.TenantId} en el contexto del inquilino {_currentTenantId}");
                }

                // Asignamos el TenantId actual a la entidad
                entity.TenantId = _currentTenantId.Value;
            }
        }
    }
} 