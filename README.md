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

# Mapa Mental del Flujo de Trabajo - AppMultiTenant

## Visión General de la Aplicación

AppMultiTenant es una aplicación web multi-inquilino (multi-tenant) que permite a múltiples organizaciones (inquilinos) operar de forma independiente en un entorno seguro, gestionando sus propios usuarios, permisos y estructuras de datos personalizadas.

## Arquitectura de la Aplicación

La aplicación sigue una arquitectura limpia con separación clara de responsabilidades:

```
┌───────────────────┐    ┌───────────────────┐    ┌────────────────────┐    ┌─────────────────┐
│                   │    │                   │    │                    │    │                 │
│ Blazor WebAssembly│────▶   ASP.NET Core   │────▶   Capa Aplicación  │────▶ Capa Dominio   │
│   (MVVM)         │    │   Web API         │    │                    │    │                 │
│                   │    │                   │    │                    │    │                 │
└───────────────────┘    └───────────────────┘    └────────────────────┘    └─────────────────┘
        │                        │                          │                       ▲
        │                        │                          │                       │
        │                        │                          │                       │
        │                        │                          ▼                       │
        │                        │                 ┌────────────────────┐           │
        │                        │                 │                    │           │
        │                        └────────────────▶│  Infraestructura  │───────────┘
        │                                          │                    │
        └─────────────────────────────────────────▶│                    │
                                                   └────────────────────┘
```

## Proyectos de la Solución

1. **`AppMultiTenant.Domain`** - Núcleo de la aplicación, contiene entidades y lógica de negocio
2. **`AppMultiTenant.Application`** - Orquesta los casos de uso utilizando las entidades del dominio
3. **`AppMultiTenant.Infrastructure`** - Implementa interfaces de aplicación y acceso a datos
4. **`AppMultiTenant.Server`** - API Web que expone la funcionalidad
5. **`AppMultiTenant.Client`** - Interfaz de usuario Blazor WebAssembly

## Flujo de Trabajo Principal

### 1. Identificación del Inquilino (Tenant)

```
Cliente Web ──▶ HTTP Request ──▶ TenantResolutionMiddleware ──▶ TenantResolverService ──▶ Contexto con TenantId
                                (AppMultiTenant.Server)       (AppMultiTenant.Infrastructure)
```

- **`TenantResolutionMiddleware`** (`AppMultiTenant.Server`): Intercepta cada solicitud HTTP para identificar a qué inquilino pertenece.
- **`TenantResolverService`** (`AppMultiTenant.Infrastructure`): Determina el `TenantId` basado en el subdominio, ruta URL o token `JWT`.

### 2. Autenticación y Autorización

```
Cliente ──▶ AuthController ──▶ AuthService ──▶ Identity ──▶ JWT ──▶ Cliente almacena token
             (Server)          (Application)   (Infrastructure)
```

- **`AuthController`** (`AppMultiTenant.Server`): Recibe credenciales de usuario y devuelve tokens `JWT`.
- **`AuthService`** (`AppMultiTenant.Application`): Orquesta el proceso de autenticación y generación de token.
- **`Identity`** (`AppMultiTenant.Infrastructure`): Valida credenciales contra la base de datos, teniendo en cuenta el `TenantId`.
- **`CustomAuthenticationStateProvider`** (`AppMultiTenant.Client`): Gestiona el estado de autenticación en el cliente basado en `JWT`.

### 3. Gestión de Usuarios (por Inquilino)

```
Cliente ──▶ TenantUsersController ──▶ TenantUserService ──▶ UserRepository ──▶ Base de Datos
             (Server)                 (Application)         (Infrastructure)    (con filtro TenantId)
```

- **`TenantUsersController`** (`AppMultiTenant.Server`): Endpoints para CRUD de usuarios dentro de un inquilino.
- **`TenantUserService`** (`AppMultiTenant.Application`): Lógica de negocio para gestionar usuarios en un inquilino.
- **`UserRepository`** (`AppMultiTenant.Infrastructure`): Acceso a datos de usuarios con filtrado automático por `TenantId`.

### 4. Gestión de Roles y Permisos (por Inquilino)

```
Cliente ──▶ TenantRolesController ──▶ TenantRoleService ──▶ RoleRepository/PermissionRepository ──▶ Base de Datos
             (Server)                 (Application)         (Infrastructure)                          (con filtro TenantId)
```

- **`TenantRolesController`** (`AppMultiTenant.Server`): Endpoints para CRUD de roles y asignación de permisos.
- **`TenantRoleService`** (`AppMultiTenant.Application`): Lógica para gestionar roles y sus permisos.
- **`RoleRepository/PermissionRepository`** (`AppMultiTenant.Infrastructure`): Acceso a datos de roles y permisos.

### 5. Gestión de Secciones de Aplicación (por Inquilino)

```
Cliente ──▶ TenantSectionDefinitionsController ──▶ TenantSectionDefinitionService ──▶ AppSectionDefinitionRepository ──▶ Base de Datos
             (Server)                              (Application)                      (Infrastructure)                    (con filtro TenantId)
```

- **`TenantSectionDefinitionsController`** (`AppMultiTenant.Server`): Endpoints para definir secciones personalizadas.
- **`TenantSectionDefinitionService`** (`AppMultiTenant.Application`): Lógica para gestionar estructuras de datos personalizadas.
- **`AppSectionDefinitionRepository`** (`AppMultiTenant.Infrastructure`): Acceso a datos de definiciones de secciones.

### 6. Gestión de Contenido Dinámico (CRUD por Sección)

```
Cliente ──▶ TenantSectionDataController ──▶ TenantSectionDataService ──▶ SectionDataEntryRepository ──▶ Base de Datos
             (Server)                       (Application)                (Infrastructure)                (con filtro TenantId)
```

- **`TenantSectionDataController`** (`AppMultiTenant.Server`): Endpoints para CRUD de datos en secciones personalizadas.
- **`TenantSectionDataService`** (`AppMultiTenant.Application`): Lógica para gestionar los datos dentro de secciones.
- **`SectionDataEntryRepository`** (`AppMultiTenant.Infrastructure`): Acceso a datos de entradas en secciones.

### 7. Administración del Sistema (Super Administrador)

```
Cliente ──▶ SystemAdminTenantsController ──▶ SystemAdminTenantService ──▶ TenantRepository ──▶ Base de Datos
             (Server)                         (Application)                (Infrastructure)
```

- **`SystemAdminTenantsController`** (`AppMultiTenant.Server`): Endpoints para que el Super Admin gestione inquilinos.
- **`SystemAdminTenantService`** (`AppMultiTenant.Application`): Lógica para la creación y gestión de inquilinos.
- **`TenantRepository`** (`AppMultiTenant.Infrastructure`): Acceso a datos de inquilinos.

## Flujo MVVM en el Cliente Blazor WebAssembly

```
┌────────────────┐      ┌────────────────┐      ┌────────────────┐
│                │      │                │      │                │
│    Vista       │◄────▶│   ViewModel    │◄────▶│  API Clients   │
│  (.razor)      │      │                │      │                │
│                │      │                │      │                │
└────────────────┘      └────────────────┘      └────────────────┘
                                                        │
                                                        ▼
                                                ┌────────────────┐
                                                │                │
                                                │   API Server   │
                                                │                │
                                                │                │
                                                └────────────────┘
```

1. **Vistas** (`AppMultiTenant.Client`):
   - Archivos `.razor` que representan la interfaz de usuario
   - Conectadas a sus ViewModels correspondientes

2. **ViewModels** (`AppMultiTenant.Client`):
   - Contienen la lógica de presentación y estado de la UI
   - Gestionan las llamadas a la API y preparan los datos para las vistas

3. **ApiClients** (`AppMultiTenant.Client`):
   - Encapsulan la comunicación HTTP con la API Backend
   - Gestionan la serialización/deserialización de entidades

## Filtrado por Inquilino en Base de Datos

Un aspecto clave de la arquitectura:

```
┌─────────────────┐     ┌────────────────┐     ┌───────────────────┐
│                 │     │                │     │                   │
│   Repository    │────▶│  AppDbContext  │────▶│  Global Filters   │────▶ SQL con WHERE TenantId = X
│                 │     │                │     │                   │
│                 │     │                │     │                   │
└─────────────────┘     └────────────────┘     └───────────────────┘
```

- **`AppDbContext`** (`AppMultiTenant.Infrastructure`): Utiliza EF Core Global Query Filters para filtrar automáticamente todas las consultas por TenantId.
- Este mecanismo garantiza que los datos de un inquilino nunca sean accesibles para otros inquilinos.

## Entidades Principales del Dominio

- **`Tenant`** (`AppMultiTenant.Domain`): Representa un inquilino en el sistema.
- **`ApplicationUser`** (`AppMultiTenant.Domain`): Usuario dentro de un inquilino específico (contiene TenantId).
- **`ApplicationRole`** (`AppMultiTenant.Domain`): Rol dentro de un inquilino específico (contiene TenantId).
- **`Permission`** (`AppMultiTenant.Domain`): Permisos que pueden asignarse a roles.
- **`AppSectionDefinition`** (`AppMultiTenant.Domain`): Define una estructura de datos personalizada para un inquilino.


## Resumen Simple

La aplicación permite que múltiples organizaciones (inquilinos) utilicen la misma plataforma mientras mantienen sus datos completamente separados. Cada inquilino puede:

1. Gestionar sus propios usuarios
2. Definir roles y permisos personalizados
3. Crear estructuras de datos personalizadas (secciones)
4. Trabajar con datos en estas secciones personalizadas

Todo esto mientras el sistema garantiza que los datos de un inquilino nunca son accesibles para otros inquilinos, gracias al filtrado automático por TenantId en todos los niveles de la aplicación.