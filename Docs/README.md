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
- **AppMultiTenant.ClientWASM**: Frontend (Blazor WebAssembly con MVVM)

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

## Proyectos de la Solución y Sus Componentes Principales

### 1. **`AppMultiTenant.Domain`** - Entidades y Lógica de Negocio
- **`Tenant`**: Representa un inquilino (organización) en el sistema.
- **`ApplicationUser`**: Usuario que pertenece a un inquilino específico.
- **`ApplicationRole`**: Rol específico de un inquilino.
- **`Permission`**: Permisos del sistema que se pueden asignar a roles.
- **`RolePermission`**: Asociación entre un rol y un permiso.
- **`AppSectionDefinition`**: Define una estructura de datos personalizada para un inquilino.
- **`ITenantEntity`**: Interfaz que deben implementar todas las entidades que pertenecen a un inquilino.

### 2. **`AppMultiTenant.Application`** - Casos de Uso y Lógica de Aplicación
- **Interfaces de Servicios**:
  - **`IAuthService`**: Maneja la autenticación de usuarios.
  - **`ITenantUserService`**: Gestiona usuarios dentro de un inquilino.
  - **`ITenantRoleService`**: Gestiona roles y permisos por inquilino.
  - **`ITenantSectionDefinitionService`**: Gestiona definiciones de secciones personalizadas.
  - **`ISystemAdminTenantService`**: Administra inquilinos a nivel de sistema.
  - **`ITenantResolverService`**: Resuelve el inquilino actual.
  - **`IEntitySerializationService`**: Maneja la serialización/deserialización segura de entidades de dominio directamente desde/hacia los controladores API.

- **Interfaces de Persistencia**:
  - **`IRepositoryBase<T>`**: Operaciones CRUD básicas para entidades.
  - **`ITenantRepository`**: Acceso a datos de inquilinos.
  - **`IUserRepository`**: Acceso a datos de usuarios.
  - **`IRoleRepository`**: Acceso a datos de roles.
  - **`IPermissionRepository`**: Acceso a datos de permisos.
  - **`IAppSectionDefinitionRepository`**: Acceso a datos de definiciones de secciones.
  - **`IUnitOfWork`**: Coordina operaciones de repositorios.

- **Implementaciones de Servicios**:
  - **`AuthService`**: Implementa `IAuthService`.
  - **`TenantUserService`**: Implementa `ITenantUserService`.
  - **`TenantRoleService`**: Implementa `ITenantRoleService`.
  - **`TenantSectionDefinitionService`**: Implementa `ITenantSectionDefinitionService`.
  - **`SystemAdminTenantService`**: Implementa `ISystemAdminTenantService`.
  - **`ValidationService`**: Validación de datos en los servicios.
  - **`EntitySerializationService`**: Implementa la serialización/deserialización segura de entidades de dominio con configuraciones de seguridad.

### 3. **`AppMultiTenant.Infrastructure`** - Implementaciones Técnicas
- **Persistencia**:
  - **`AppDbContext`**: Contexto de Entity Framework Core que aplica filtrado por inquilino.
  - **`RepositoryBase<T>`**: Implementación base de repositorios genéricos.
  - **`TenantRepository`**: Implementa `ITenantRepository`.
  - **`UserRepository`**: Implementa `IUserRepository`.
  - **`RoleRepository`**: Implementa `IRoleRepository`.
  - **`PermissionRepository`**: Implementa `IPermissionRepository`.
  - **`AppSectionDefinitionRepository`**: Implementa `IAppSectionDefinitionRepository`.
  - **`UnitOfWork`**: Implementa `IUnitOfWork`.

- **Identidad**:
  - **`TenantResolverService`**: Resuelve el inquilino desde subdominios, rutas, cabeceras HTTP o JWT.
  - **`MultiTenantUserStore`**: Adaptación de Identity para soportar inquilinos.
  - **`MultiTenantRoleStore`**: Adaptación de Identity para roles por inquilino.
  - **`JwtConfiguration`**: Configuración para tokens JWT.
  - **`IdentityConfiguration`**: Configuración general de Identity.

### 4. **`AppMultiTenant.Server`** - API Web
- **`TenantResolutionMiddleware`**: Middleware que resuelve el inquilino para cada solicitud.
- **`GlobalExceptionHandlingMiddleware`**: Middleware que captura todas las excepciones no controladas y las transforma en respuestas HTTP estructuradas y consistentes.
- **`Program.cs`**: Configuración de la aplicación, servicios y middleware.
- **Controladores**:
  - **`AuthController`**: Maneja autenticación y registro de usuarios.
  - **`SystemAdminTenantsController`**: Gestión de inquilinos por el Super Administrador.
  - **`TenantUsersController`**: Gestión de usuarios dentro de un inquilino.
  - **`TenantRolesController`**: Gestión de roles y permisos dentro de un inquilino.
  - **`TenantSectionDefinitionsController`**: Gestión de definiciones de secciones por el Administrador de Inquilino.

### 5. **`AppMultiTenant.ClientWASM`** - Interfaz de Usuario
- **`Components/`**: Componentes Blazor para la interfaz.
  - **`Layout/`**: Componentes de diseño de la aplicación:
    - **`MainLayout.razor`**: Layout principal para usuarios normales.
    - **`SuperAdminLayout.razor`**: Layout específico para Super Administradores con tema y navegación distintos.
    - **`NavMenu.razor`**: Menú de navegación principal.
    - **`SuperAdminNavMenu.razor`**: Menú de navegación específico para Super Administradores.
  - **`Routes.razor`**: Enrutamiento dinámico que aplica diferentes layouts según la ruta.
  - **`Pages/`**: Páginas de la aplicación organizadas en módulos:
    - **`Auth/`**: Componentes para autenticación (Login, Register).
    - **`Users/`**: Gestión de usuarios del inquilino.
    - **`Roles/`**: Gestión de roles y permisos.
    - **`SectionDefinitions/`**: Gestión de definiciones de secciones.
    - **`Tenants/`**: Gestión de inquilinos (Super Administrador).
    - **`Home.razor`**: Dashboard principal.
- **`ViewModels/`**: Implementaciones MVVM para la lógica de presentación.
- **`Services/`**: Servicios del cliente para comunicación con la API.
- **`State/`**: Gestión del estado de la aplicación cliente.

## Flujo de Trabajo Detallado

### 1. Identificación del Inquilino (Tenant)

```
Cliente Web ──▶ HTTP Request ──▶ TenantResolutionMiddleware ──▶ TenantResolverService ──▶ Contexto con TenantId
                                (AppMultiTenant.Server)       (AppMultiTenant.Infrastructure)
```

1. El cliente web hace una solicitud HTTP al servidor.
2. **`TenantResolutionMiddleware`** (`AppMultiTenant.Server`):
   - Intercepta cada solicitud HTTP.
   - Llama a `ITenantResolverService` para determinar el inquilino actual.
   - Almacena el `TenantId` y `TenantIdentifier` en `HttpContext.Items`.

3. **`TenantResolverService`** (`AppMultiTenant.Infrastructure`):
   - Implementa múltiples estrategias para identificar el inquilino:
     - Por subdominio (ej. `tenant1.app.com`)
     - Por ruta URL (ej. `/tenant1/...`)
     - Por cabecera HTTP
     - Por claims en el token JWT
   - Almacena el inquilino identificado en un contexto AsyncLocal para disponibilidad en toda la solicitud.

4. El `TenantId` resuelto se utilizará en todas las operaciones subsiguientes para garantizar el aislamiento de datos.

### 2. Autenticación y Autorización

```
Cliente ──▶ AuthController ──▶ AuthService ──▶ Identity ──▶ JWT ──▶ Cliente almacena token
             (Server)          (Application)   (Infrastructure)

Solicitud ──▶ JWT ──▶ TenantResolutionMiddleware ──▶ Controllers ──┐
Autenticada           GlobalExceptionHandlingMiddleware            │
                                                                   ▼
                      ◀── Resultado ◀── TenantAuthorizationHandler ◀──┐
                      │                 PermissionAuthorizationHandler │
                      │                                                │
                      └────────────────────────────────────────────────┘
```

#### Flujo de Autenticación:

1. El usuario envía credenciales a través del cliente.
2. **`AuthController`** (`AppMultiTenant.Server`):
   - Recibe las credenciales del usuario.
   - Llama a `IAuthService.LoginAsync()`.

3. **`AuthService`** (`AppMultiTenant.Application`):
   - Valida las credenciales contra Identity.
   - Genera un token JWT con claims de usuario, inquilino y permisos.
   - Devuelve el token y la información del usuario.

4. **Identity** (`AppMultiTenant.Infrastructure`):
   - `MultiTenantUserStore`: Adapta Identity para filtrar por `TenantId`.
   - `MultiTenantRoleStore`: Gestiona roles específicos por inquilino.

5. El cliente almacena el token JWT para usarlo en solicitudes futuras.

#### Flujo de Autorización:

1. El cliente envía una solicitud con el token JWT.
2. El middleware de autenticación valida el token y establece el contexto de usuario.
3. **`TenantResolutionMiddleware`** resuelve y establece el `TenantId` en el contexto.
4. Cuando la solicitud llega a un controlador con atributos `[Authorize]`:
   - **`TenantAuthorizationHandler`**: Verifica que el `TenantId` del token coincida con el `TenantId` resuelto.
   - **`PermissionAuthorizationHandler`**: Verifica que el usuario tenga los permisos necesarios.

5. Los controladores aplican políticas de autorización específicas:
   - **`RequireTenantAccess`**: Asegura que el usuario sólo acceda a datos de su inquilino.
   - Políticas basadas en permisos: "CreateUser", "EditRole", "DeleteUser", etc.
   
6. Si alguna verificación falla, se devuelve un error 403 (Forbidden); de lo contrario, se procesa la solicitud.

### 3. Gestión de Usuarios (por Inquilino)

```
Cliente ──▶ TenantUsersController ──▶ TenantUserService ──▶ UserRepository ──▶ Base de Datos
             (Server)                 (Application)         (Infrastructure)    (con filtro TenantId)
```

1. **`TenantUsersController`** (`AppMultiTenant.Server`):
   - Expone endpoints para CRUD de usuarios dentro de un inquilino.
   - Proporciona funcionalidades para listar, crear, actualizar y eliminar usuarios.
   - Permite gestionar datos de usuario como nombre, email, estado activo/inactivo.
   - Soporta la asignación y eliminación de roles a usuarios.
   - Obtiene el `TenantId` automáticamente del contexto HTTP.
   - Llama a métodos de `ITenantUserService`.

2. **`TenantUserService`** (`AppMultiTenant.Application`):
   - Implementa la lógica de negocio para gestionar usuarios.
   - Utiliza `IUserRepository` para operaciones de datos.
   - Valida que las operaciones respeten el contexto del inquilino actual.

3. **`UserRepository`** (`AppMultiTenant.Infrastructure`):
   - Implementa `IUserRepository`.
   - Utiliza `AppDbContext` para operaciones de base de datos.
   - Se beneficia del filtrado automático por `TenantId` en `AppDbContext`.

### 4. Gestión de Roles y Permisos (por Inquilino)

```
Cliente ──▶ TenantRolesController ──▶ TenantRoleService ──▶ RoleRepository/PermissionRepository ──▶ Base de Datos
             (Server)                 (Application)         (Infrastructure)                          (con filtro TenantId)
```

1. **`TenantRolesController`** (`AppMultiTenant.Server`):
   - Expone endpoints para CRUD de roles y asignación de permisos dentro de un inquilino.
   - Proporciona funcionalidades para listar, crear, actualizar y eliminar roles.
   - Permite gestionar datos de roles como nombre y descripción.
   - Implementa verificación de disponibilidad de nombres de roles.
   - Permite obtener todos los permisos disponibles en el sistema.
   - Soporta la asignación y eliminación de permisos a roles.
   - Obtiene el `TenantId` automáticamente del contexto HTTP.
   - Llama a métodos de `ITenantRoleService`.

2. **`TenantRoleService`** (`AppMultiTenant.Application`):
   - Implementa la lógica para gestionar roles y sus permisos.
   - Utiliza `IRoleRepository` y `IPermissionRepository`.
   - Maneja la creación/eliminación de asociaciones `RolePermission`.

3. **`RoleRepository/PermissionRepository`** (`AppMultiTenant.Infrastructure`):
   - Implementan las interfaces correspondientes.
   - Utilizan `AppDbContext` con filtrado automático por `TenantId`.

### 5. Gestión de Secciones de Aplicación (por Inquilino)

```
Cliente ──▶ TenantSectionDefinitionsController ──▶ TenantSectionDefinitionService ──▶ AppSectionDefinitionRepository ──▶ Base de Datos
             (Server)                              (Application)                      (Infrastructure)                    (con filtro TenantId)
```

1. **`TenantSectionDefinitionsController`** (`AppMultiTenant.Server`):
   - Expone endpoints CRUD para definir estructuras de datos personalizadas.
   - Proporciona funcionalidades para crear, listar, buscar, actualizar y eliminar definiciones de sección.
   - Implementa verificación de disponibilidad de nombres de secciones.
   - Soporta la creación y asignación de permisos específicos para las secciones.
   - Llama a métodos de `ITenantSectionDefinitionService`.

2. **`TenantSectionDefinitionService`** (`AppMultiTenant.Application`):
   - Implementa la lógica para definir y gestionar secciones personalizadas.
   - Utiliza `IAppSectionDefinitionRepository`.
   - Gestiona la creación de permisos específicos para las secciones definidas.

3. **`AppSectionDefinitionRepository`** (`AppMultiTenant.Infrastructure`):
   - Implementa `IAppSectionDefinitionRepository`.
   - Utiliza `AppDbContext` con filtrado automático por `TenantId`.

### 6. Gestión de Contenido Dinámico (CRUD por Sección)

Esta funcionalidad está pendiente de implementar según el plan del proyecto.

### 7. Administración del Sistema (Super Administrador)

```
Cliente ──▶ SystemAdminTenantsController ──▶ SystemAdminTenantService ──▶ TenantRepository ──▶ Base de Datos
             (Server)                         (Application)                (Infrastructure)
```

1. **`SystemAdminTenantsController`** (`AppMultiTenant.Server`):
   - Expone endpoints CRUD para la gestión de inquilinos.
   - Incluye funcionalidades para crear, listar, buscar, actualizar, activar/desactivar y eliminar inquilinos.
   - Proporciona soporte para crear automáticamente un administrador inicial al crear un inquilino.
   - Llama a métodos de `ISystemAdminTenantService`.

2. **`SystemAdminTenantService`** (`AppMultiTenant.Application`):
   - Implementa la lógica para crear y gestionar inquilinos.
   - Utiliza `ITenantRepository` para operaciones de datos.
   - Configura inquilinos iniciales con administradores por defecto.

3. **`TenantRepository`** (`AppMultiTenant.Infrastructure`):
   - Implementa `ITenantRepository`.
   - Utiliza `AppDbContext` para operaciones de base de datos.
   - No aplica filtrado por `TenantId` para la entidad `Tenant`.

4. **Interfaz de Super Administrador**:
   - **`SuperAdminLayout.razor`**: Layout específico con tema distinto para indicar claramente modo Super Administrador.
   - **`SuperAdminNavMenu.razor`**: Menú de navegación especializado (SuperAdminNavMenu) con opciones relevantes para administración del sistema.
   - Aplicación automática del layout específico para rutas que comienzan con `/superadmin/`.
   - Implementación de la separación visual clara entre la interfaz de Super Administrador y la de usuarios normales.
   - Navegación específica y simplificada para las tareas del Super Administrador.

## Filtrado por Inquilino en Base de Datos - El Corazón del Multi-tenant

Un aspecto clave de la arquitectura:

```
┌─────────────────┐     ┌────────────────┐     ┌───────────────────────────┐
│                 │     │                │     │                           │
│   Repository    │────▶│  AppDbContext  │────▶│  Entity.HasQueryFilter    │────▶ SQL con WHERE TenantId = X
│                 │     │                │     │  (Global Query Filters)   │
│                 │     │                │     │                           │
└─────────────────┘     └────────────────┘     └───────────────────────────┘
```

- **`AppDbContext`** (`AppMultiTenant.Infrastructure`):
  - Recibe el `ITenantResolverService` mediante inyección de dependencias.
  - Obtiene el `TenantId` actual para la solicitud.
  - En `OnModelCreating()`, aplica `HasQueryFilter()` a cada entidad que implementa `ITenantEntity`.
  - Sobrescribe `SaveChanges()` y `SaveChangesAsync()` para asignar automáticamente el `TenantId` a entidades nuevas.

- **`RepositoryBase<T>`** (`AppMultiTenant.Infrastructure`):
  - Implementación base para todos los repositorios.
  - Utiliza `AppDbContext` para las operaciones CRUD.
  - Se beneficia del filtrado automático sin tener que aplicar filtros manualmente.

Este mecanismo garantiza que:
1. Las consultas **sólo devuelven datos** del inquilino actual.
2. Las entidades nuevas **se asignan automáticamente** al inquilino actual.
3. El código de repositorio permanece **simple y libre de lógica de filtrado manual**.

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

1. **Vistas** (`AppMultiTenant.ClientWASM`):
   - Archivos `.razor` que representan la interfaz de usuario.
   - Enlazados a ViewModels correspondientes mediante inyección de dependencias.
   - Muestran datos y responden a interacciones del usuario.

2. **ViewModels** (`AppMultiTenant.ClientWASM`):
   - Implementan propiedades y comandos para las vistas.
   - Gestionan el estado de la UI y la validación.
   - Utilizan los ApiClients para comunicarse con el backend.

3. **ApiClients** (`AppMultiTenant.ClientWASM`):
   - Encapsulan la comunicación HTTP con la API.
   - Gestionan el token JWT para autenticación.
   - Serializan/deserializan entidades del dominio.
   - Manejan errores de comunicación y respuestas de la API.

## Resumen Simple

La aplicación permite que múltiples organizaciones (inquilinos) utilicen la misma plataforma mientras mantienen sus datos completamente separados. Cada inquilino puede:

1. **Gestionar sus propios usuarios**: Cada inquilino tiene su propio conjunto de usuarios que solo pueden acceder a los datos de ese inquilino.

2. **Definir roles y permisos personalizados**: Los administradores de cada inquilino pueden crear roles específicos y asignarles permisos granulares.

3. **Crear estructuras de datos personalizadas**: Cada inquilino puede definir sus propias "secciones" con los campos que necesite para sus datos.

4. **Trabajar con datos en estas secciones personalizadas**: Los usuarios pueden crear, ver, editar y eliminar datos en las secciones a las que tienen acceso.

Todo esto mientras el sistema garantiza que los datos de un inquilino nunca son accesibles para otros inquilinos, gracias al filtrado automático por `TenantId` en todos los niveles de la aplicación.

## Estado Actual del Proyecto

El proyecto se encuentra actualmente en un estado avanzado de desarrollo:

### Completado:

- **Fase 0: Cimientos y Configuración Inicial**
  - Creación de la estructura del proyecto y configuración básica
  - Implementación del núcleo multi-inquilino básico

- **Fase 1: Modelado del Dominio (Backend)**
  - Definición completa de todas las entidades del dominio
  - Implementación de interfaces y validaciones básicas

- **Fase 2: Núcleo de la Capa de Aplicación (Backend)**
  - Definición de interfaces de servicios y repositorios
  - Implementación de todos los servicios principales de aplicación

- **Fase 3: Implementación de la Infraestructura (Backend)**
  - Configuración completa de Entity Framework Core con filtrado por inquilino
  - Implementación de todos los repositorios
  - Configuración de Identity y JWT para autenticación

- **Fase 4: Desarrollo de la API Backend**
  - Implementación de todos los controladores principales:
    - AuthController
    - SystemAdminTenantsController
    - TenantUsersController
    - TenantRolesController
    - TenantSectionDefinitionsController
  - Configuración de middleware para resolución de inquilinos y manejo de errores
  - Implementación de políticas de autorización y validación de modelos

- **Fase 5: Desarrollo de la Capa de Presentación Cliente**
  - Implementación del dashboard principal
  - Configuración de la autenticación basada en JWT
  - Implementación de los módulos:
    - Autenticación y autorización
    - Gestión de usuarios
    - Gestión de roles y permisos
    - Gestión de definiciones de secciones
    - Administración de inquilinos (Super Administrador)
  - Implementación de interfaces específicas para Super Administrador y usuarios normales

### Pendiente:

- **Fase 6: Estrategia de Pruebas**
  - Configuración de proyectos de pruebas unitarias e integración
  - Implementación de pruebas para componentes críticos

- **Fase 7: Aspectos Transversales y Refinamiento**
  - Implementación exhaustiva de validación de entradas
  - Revisión de seguridad completa
  - Optimización de rendimiento

- **Fase 8: Despliegue**
  - Configuración de pipelines CI/CD
  - Configuración de entornos de despliegue
  - Implementación de estrategias de migración de base de datos

- **Fase 9: Monitorización y Mantenimiento Continuo**
  - Configuración de herramientas de monitorización
  - Establecimiento de procesos de mantenimiento regular

### Funcionalidades que quedaron fuera del alcance inicial:

- Gestión completa de Contenido Dinámico (CRUD por Sección)
- Módulo completo de Definición de Campos para Secciones
- Entidad SectionDataEntry y FieldValue o alternativa JSON

Estas funcionalidades están planificadas para futuras iteraciones del proyecto.