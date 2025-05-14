# Migraciones de Entity Framework Core

Este directorio contiene las migraciones de Entity Framework Core para la aplicación AppMultiTenant. Las migraciones son utilizadas para crear y actualizar el esquema de la base de datos de forma controlada y versionada.

## Requisitos Previos

- .NET SDK 9.0 o superior
- Herramienta global dotnet-ef: `dotnet tool install --global dotnet-ef`
- PostgreSQL 13 o superior

## Cómo Usar las Migraciones

### Usando el Script de Migraciones

Se proporciona un script PowerShell para facilitar la gestión de migraciones en `/scripts/ef-migrations.ps1`.

**Agregar una nueva migración:**
```powershell
cd scripts
.\ef-migrations.ps1 -action add -name NombreDeLaMigración
```

**Actualizar la base de datos a la última migración:**
```powershell
cd scripts
.\ef-migrations.ps1 -action update
```

**Listar migraciones:**
```powershell
cd scripts
.\ef-migrations.ps1 -action list
```

**Eliminar la última migración:**
```powershell
cd scripts
.\ef-migrations.ps1 -action remove
```

### Usando CLI de Entity Framework Core Directamente

Si prefieres usar los comandos de EF Core directamente, aquí están las instrucciones equivalentes:

**Agregar una nueva migración:**
```bash
dotnet ef migrations add NombreDeLaMigración --project AppMultiTenant.Infrastructure --startup-project AppMultiTenant.Server --output-dir Persistence/Migrations
```

**Actualizar la base de datos:**
```bash
dotnet ef database update --project AppMultiTenant.Infrastructure --startup-project AppMultiTenant.Server
```

**Listar migraciones:**
```bash
dotnet ef migrations list --project AppMultiTenant.Infrastructure --startup-project AppMultiTenant.Server
```

**Generar script SQL:**
```bash
dotnet ef migrations script --project AppMultiTenant.Infrastructure --startup-project AppMultiTenant.Server
```

## Estructura de la Base de Datos

La aplicación utiliza un enfoque multi-inquilino con base de datos compartida y discriminador `TenantId`. Las principales tablas incluyen:

- `Tenants`: Inquilinos del sistema
- `Users`, `Roles`, etc.: Tablas de ASP.NET Identity (personalizadas con TenantId)
- `Permissions`, `RolePermissions`: Sistema de permisos personalizado
- `SectionDefinitions`: Definiciones de secciones de aplicación por inquilino

## Estrategia de Migraciones en Entornos de Producción

Para aplicar migraciones en entornos de producción, se recomienda:

1. **Pruebas previas**: Aplicar la migración en un entorno de staging idéntico a producción.
2. **Generación de script**: Usar `dotnet ef migrations script` para generar un script SQL de actualización.
3. **Respaldo**: Realizar una copia de seguridad completa de la base de datos antes de aplicar la migración.
4. **Ventana de mantenimiento**: Aplicar las migraciones durante una ventana de mantenimiento planificada.
5. **Rollback**: Tener un plan de rollback en caso de problemas.

## Solución de Problemas

- **Error de conexión**: Verifica la cadena de conexión en `appsettings.json` o usa la opción `-connection` del script.
- **Tabla no existe**: Asegúrate de que has aplicado la migración inicial.
- **Conflictos de migración**: Si hay conflictos, considera eliminar la última migración con `-action remove` y volver a crearla. 