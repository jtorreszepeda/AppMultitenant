# AppMultiTenant - Aplicación Web Multi-Inquilino

Aplicación web multi-inquilino (multi-tenant) desarrollada con .NET y Blazor WebAssembly siguiendo los principios de Clean Architecture.

## Descripción

Este proyecto implementa una plataforma SaaS (Software as a Service) que permite a múltiples organizaciones (inquilinos) gestionar información de forma estructurada y personalizada. Cada organización opera en su propio espacio aislado con control sobre sus usuarios y datos.

### Características principales

- Gestión centralizada de inquilinos
- Gestión de usuarios por inquilino
- Sistema de autenticación y autorización por inquilino
- Definición de roles y permisos granulares por inquilino
- Creación dinámica de secciones de datos y sus campos por inquilino
- Funcionalidad CRUD para gestionar datos dentro de las secciones definidas

## Estructura del proyecto

- **AppMultiTenant.Domain**: Entidades, lógica de negocio central, interfaces de dominio
- **AppMultiTenant.Application**: Casos de uso, interfaces de servicios, interfaces de repositorio
- **AppMultiTenant.Infrastructure**: Implementación de interfaces de Aplicación y Dominio
- **AppMultiTenant.Server**: API Backend (ASP.NET Core Web API)
- **AppMultiTenant.Client**: Frontend (Blazor WebAssembly con MVVM)

## Requisitos previos

- .NET 9 (o versión LTS más reciente)
- PostgreSQL
- IDE (Visual Studio 2022 recomendado)

## Instalación y configuración

1. Clonar el repositorio
```
git clone [URL-del-repositorio]
```

2. Restaurar paquetes NuGet
```
dotnet restore
```

3. Configurar la cadena de conexión a la base de datos en `AppMultiTenant.Server/appsettings.json`

4. Configurar las opciones de multi-inquilino en `appsettings.json`:
```json
{
  "TenantConfiguration": {
    "TenantResolutionStrategy": "Header", 
    "DefaultTenantId": "", 
    "DevModeEnabled": true
  }
}
```

5. Aplicar migraciones de Entity Framework Core
```
cd AppMultiTenant.Server
dotnet ef database update
```

6. Ejecutar el proyecto
```
dotnet run --project AppMultiTenant.Server
```

## Desarrollo

- Para agregar una nueva migración:
```
dotnet ef migrations add [NombreMigracion] --project AppMultiTenant.Infrastructure --startup-project AppMultiTenant.Server
```

- Para actualizar la base de datos:
```
dotnet ef database update --project AppMultiTenant.Infrastructure --startup-project AppMultiTenant.Server
```

## Arquitectura

El proyecto sigue una arquitectura limpia (Clean Architecture) con las siguientes capas:

1. **Dominio**: El núcleo de la aplicación que contiene las entidades y la lógica de negocio
2. **Aplicación**: Orquesta el flujo de datos entre el dominio y la infraestructura
3. **Infraestructura**: Implementa los detalles técnicos (base de datos, autenticación, etc.)
4. **Presentación**: Divide en dos partes - API Backend y Frontend Blazor

En el frontend se implementa el patrón MVVM (Model-View-ViewModel).

### Dependencias entre capas

Las dependencias entre capas siguen estrictamente la regla de dependencia de Clean Architecture:

```
Client/Server → Infrastructure → Application → Domain
```

Esto significa que:
- Domain no depende de ninguna otra capa
- Application solo depende de Domain
- Infrastructure depende de Application (y por herencia de Domain)
- Server/Client dependen de Infrastructure (y por herencia de Application y Domain)

## Implementación Multi-Inquilino

### Estrategia de Base de Datos

La aplicación implementa una estrategia de base de datos compartida con esquema compartido, utilizando un discriminador `TenantId` para aislar los datos de cada inquilino. Esta estrategia ofrece:

- **Eficiencia en recursos**: Una sola base de datos para todos los inquilinos
- **Simplicidad de mantenimiento**: Un solo esquema de datos para mantener y actualizar
- **Aislamiento lógico**: Cada inquilino solo puede acceder a sus propios datos

### Componentes Clave para Multi-Inquilino

#### 1. Interfaz ITenantEntity

Todas las entidades que pertenecen a un inquilino específico implementan esta interfaz, que define:

```csharp
public interface ITenantEntity
{
    Guid TenantId { get; set; }
}
```

#### 2. AppDbContext Multi-Inquilino

El `AppDbContext` se ha configurado para soportar multi-inquilino mediante:

- **Constructor con ITenantResolverService**: Obtiene automáticamente el inquilino actual del contexto de la solicitud
- **Global Query Filters**: Filtran automáticamente las consultas por `TenantId`
- **SaveChanges mejorado**: Asigna automáticamente el `TenantId` actual a las entidades nuevas
- **Validación de asignación**: Evita guardar entidades con `TenantId` incorrecto

#### 3. ITenantResolverService

Interfaz responsable de determinar el inquilino actual en el contexto de una solicitud:

```csharp
public interface ITenantResolverService
{
    Guid? GetCurrentTenantId();
    string? GetCurrentTenantIdentifier();
}
```

#### 4. TenantResolverService

Implementación de `ITenantResolverService` que:

- Recupera el inquilino actual del contexto HTTP (actualmente mediante cabeceras HTTP)
- Almacena el contexto del inquilino durante toda la solicitud usando `AsyncLocal<T>`
- Permite establecer manualmente el inquilino actual para pruebas
- Soporta diferentes estrategias de resolución configurables:
  - Cabecera HTTP (implementación inicial)
  - Subdominio (futura implementación)
  - Segmento de ruta (futura implementación)
  - Claim JWT (futura implementación)

Ejemplo de uso con cabeceras HTTP:
```
GET /api/data HTTP/1.1
Host: example.com
X-TenantId: 3fa85f64-5717-4562-b3fc-2c963f66afa6
X-TenantIdentifier: inquilino-1
```

#### 5. Configuración de la infraestructura multi-inquilino

Para habilitar la resolución de inquilinos, se deben registrar los siguientes servicios:

```csharp
// En Program.cs
services.AddHttpContextAccessor();
services.AddScoped<ITenantResolverService, TenantResolverService>();
```

### Consideraciones de dependencias

Durante el desarrollo, se identificaron requisitos de paquetes necesarios para que la infraestructura multi-inquilino funcione correctamente:

```xml
<!-- Para AppMultiTenant.Infrastructure -->
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" Version="2.2.0" />
  <PackageReference Include="Microsoft.Extensions.Options" Version="9.0.4" />
</ItemGroup>
```

Para evitar dependencias circulares, la clase `TenantConfiguration` debe colocarse en la capa de Application en lugar de Server, permitiendo que tanto Infrastructure como Server accedan a ella sin crear referencias circulares.

Esta arquitectura garantiza que los datos permanezcan estrictamente separados entre inquilinos, aun compartiendo las mismas tablas en la base de datos.

## Registro de cambios

### 2025-05-09
- Implementación de la interfaz `ITenantResolverService` y su implementación básica `TenantResolverService` en `AppMultiTenant.Infrastructure/Identity`
- Registro del servicio `TenantResolverService` en la inyección de dependencias
- Identificación y documentación de dependencias necesarias para la implementación multi-inquilino 