using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AppMultiTenant.Domain.Entities;
using AppMultiTenant.Application.Interfaces.Services;

namespace AppMultiTenant.Infrastructure.Persistence
{
    /// <summary>
    /// Contexto de base de datos principal de la aplicación multi-inquilino.
    /// Implementa el enfoque de base de datos compartida con esquema compartido y discriminador TenantId.
    /// </summary>
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
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
        /// Constructor para uso en pruebas o migración (NO usar con DI)
        /// </summary>
        /// <param name="options">Opciones del DbContext</param>
        /// <param name="tenantId">ID del inquilino o null para el modo admin global</param>
        public AppDbContext(
            DbContextOptions<AppDbContext> options, 
            Guid? tenantId) : base(options)
        {
            _currentTenantId = tenantId;
        }

        /// <summary>
        /// Colección de inquilinos en el sistema
        /// </summary>
        public DbSet<Tenant> Tenants => Set<Tenant>();

        /// <summary>
        /// Colección de permisos en el sistema
        /// </summary>
        public DbSet<Permission> Permissions => Set<Permission>();

        /// <summary>
        /// Colección de relaciones entre roles y permisos
        /// </summary>
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        /// <summary>
        /// Colección de definiciones de secciones de aplicación
        /// </summary>
        public DbSet<AppSectionDefinition> SectionDefinitions => Set<AppSectionDefinition>();

        /// <summary>
        /// Configura el modelo y aplica los Global Query Filters para el aislamiento de datos por inquilino
        /// </summary>
        /// <param name="modelBuilder">Builder para el modelo de EF Core</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurar el esquema de la base de datos
            modelBuilder.HasDefaultSchema("app_multi_tenant");

            base.OnModelCreating(modelBuilder);

            // Personalizar nombres de tablas de Identity
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<ApplicationRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

            // Configuración para la entidad Tenant
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.ToTable("Tenants");
                entity.HasKey(e => e.TenantId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Identifier).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Identifier).IsUnique();
            });

            // Configuración para la entidad ApplicationUser
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                // Propiedades adicionales a las de Identity
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.TenantId).IsRequired();
                entity.Property(e => e.CreatedDate).IsRequired();
                
                // Relación con Tenant
                entity.HasOne<Tenant>().WithMany().HasForeignKey(e => e.TenantId);
                
                // Aplicar Global Query Filter para TenantId
                entity.HasQueryFilter(e => _currentTenantId == null || e.TenantId == _currentTenantId);
            });

            // Configuración para la entidad ApplicationRole
            modelBuilder.Entity<ApplicationRole>(entity =>
            {
                // Propiedades adicionales a las de Identity
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.TenantId).IsRequired();
                entity.Property(e => e.CreatedDate).IsRequired();
                
                // Relación con Tenant
                entity.HasOne<Tenant>().WithMany().HasForeignKey(e => e.TenantId);
                
                // Aplicar Global Query Filter para TenantId
                entity.HasQueryFilter(e => _currentTenantId == null || e.TenantId == _currentTenantId);
            });

            // Configuración para la entidad Permission
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.ToTable("Permissions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Configuración para la entidad RolePermission
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.ToTable("RolePermissions");
                entity.HasKey(e => new { e.Id, e.PermissionId });
                entity.Property(e => e.TenantId).IsRequired();
                
                // Clarificando en los comentarios que Id es en realidad RoleId
                // Relaciones
                entity.HasOne(e => e.Role)
                      .WithMany()
                      .HasForeignKey(e => e.Id) // Id es RoleId en RolePermission
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Permission)
                      .WithMany()
                      .HasForeignKey(e => e.PermissionId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Aplicar Global Query Filter para TenantId
                entity.HasQueryFilter(e => _currentTenantId == null || e.TenantId == _currentTenantId);
            });

            // Configuración para la entidad AppSectionDefinition
            modelBuilder.Entity<AppSectionDefinition>(entity =>
            {
                entity.ToTable("SectionDefinitions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NormalizedName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.TenantId).IsRequired();
                
                // Relación con Tenant
                entity.HasOne<Tenant>().WithMany().HasForeignKey(e => e.TenantId);
                
                // Índice compuesto para evitar nombres duplicados dentro de un inquilino
                entity.HasIndex(e => new { e.TenantId, e.NormalizedName }).IsUnique();
                
                // Aplicar Global Query Filter para TenantId
                entity.HasQueryFilter(e => _currentTenantId == null || e.TenantId == _currentTenantId);
            });
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