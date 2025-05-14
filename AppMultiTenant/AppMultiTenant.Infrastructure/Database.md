# Configuración de Base de Datos Multi-Inquilino

Este documento describe la configuración de la base de datos para el proyecto AppMultiTenant, enfocado en la implementación multi-inquilino.

## Enfoque Multi-Inquilino

El proyecto implementa un enfoque de **base de datos compartida con esquema compartido y discriminador TenantId**, donde:

- Todos los inquilinos comparten la misma base de datos PostgreSQL
- Las tablas incluyen una columna `TenantId` que identifica a qué inquilino pertenece cada registro
- Entity Framework Core aplica filtros globales de consulta para aislar los datos automáticamente

## Configuración de Entity Framework Core

### AppDbContext

La clase `AppDbContext` es el punto central de la configuración de Entity Framework Core:

- Extiende `IdentityDbContext` para integrar ASP.NET Identity
- Implementa filtrado automático por inquilino mediante Global Query Filters
- Asigna automáticamente el TenantId actual a nuevas entidades
- Personaliza el esquema de base de datos en el método `OnModelCreating`

### Migraciones

Las migraciones de Entity Framework Core se utilizan para crear y mantener el esquema de la base de datos:

1. Las migraciones se encuentran en `AppMultiTenant.Infrastructure/Persistence/Migrations/`
2. Scripts útiles están disponibles en el directorio `scripts/`:
   - `InitializeMigrations.ps1`: Crea y aplica la migración inicial
   - `ef-migrations.ps1`: Facilita la gestión de migraciones (agregar, actualizar, listar, eliminar)

Para inicializar la base de datos por primera vez:

```powershell
cd scripts
.\InitializeMigrations.ps1
```

## Cadena de Conexión

La cadena de conexión a la base de datos se configura en:

1. `appsettings.json` para valores predeterminados
2. `appsettings.Development.json` para entorno de desarrollo
3. User Secrets (recomendado para desarrollo local)
4. Variables de entorno (recomendado para producción)

Ejemplo de cadena de conexión para PostgreSQL:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=AppMultiTenant;Username=postgres;Password=YourStrongPassword"
}
```

## Modelo de Datos Multi-Inquilino

### Entidades con Tenant

Las entidades que implementan `ITenantEntity` tienen filtrado automático por inquilino:

- ApplicationUser
- ApplicationRole
- RolePermission
- AppSectionDefinition

### Entidades Globales

Las entidades que no implementan `ITenantEntity` son accesibles desde todos los inquilinos:

- Tenant (definiciones de inquilinos)
- Permission (permisos globales del sistema)

## Administración de PostgreSQL

### Instalación Local

Para desarrollo local, se recomienda:

1. [PostgreSQL 13 o superior](https://www.postgresql.org/download/)
2. [pgAdmin 4](https://www.pgadmin.org/download/) como herramienta de administración gráfica

### Scripts de Inicialización

Cuando se ejecuta la primera migración, Entity Framework Core crea automáticamente:

1. La base de datos (si no existe)
2. El esquema definido en las migraciones
3. Datos iniciales configurados en las migraciones

## Solución de Problemas

### Errores de Conexión

Si ocurren errores de conexión:

1. Verificar que PostgreSQL esté ejecutándose
2. Verificar credenciales de conexión
3. Verificar que el usuario tenga permisos para crear/modificar bases de datos

### Errores de Migración

Si ocurren errores al aplicar migraciones:

1. Verificar que no haya tablas con el mismo nombre creadas manualmente
2. Limpiar la base de datos si es posible y reiniciar desde cero
3. Consultar el archivo de registro de migraciones para detalles

### Rendimiento

Para optimizar el rendimiento:

1. Asegurarse de que la columna `TenantId` tenga índices apropiados
2. Monitorizar consultas lentas con herramientas como pgAdmin o pg_stat_statements
3. Considerar particionamiento por inquilino para bases de datos muy grandes 