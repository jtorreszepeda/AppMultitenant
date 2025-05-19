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
- **`Controllers/`**: Controladores REST que expondrán los servicios de la aplicación (pendientes de implementar).

### 5. **`AppMultiTenant.Client`** - Interfaz de Usuario
- **`Components/`**: Componentes Blazor para la interfaz.
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

## Flujo MVVM en el Cliente Blazor WebAssembly (Planificado)

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
   - Archivos `.razor` que representan la interfaz de usuario.
   - Enlazados a ViewModels correspondientes mediante inyección de dependencias.
   - Muestran datos y responden a interacciones del usuario.

2. **ViewModels** (`AppMultiTenant.Client`):
   - Implementan propiedades y comandos para las vistas.
   - Gestionan el estado de la UI y la validación.
   - Utilizan los ApiClients para comunicarse con el backend.

3. **ApiClients** (`AppMultiTenant.Client`):
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

Según la lista de tareas, el proyecto se encuentra en las siguientes fases:

### Backend (Completado):
- La definición de entidades del dominio
- Las interfaces e implementaciones de servicios de aplicación
- La configuración de la infraestructura y base de datos con soporte multi-inquilino
- La implementación de la resolución de inquilinos
- La implementación de controladores API:
  - AuthController para autenticación
  - SystemAdminTenantsController para gestión de inquilinos por el Super Administrador
  - TenantUsersController para gestión de usuarios dentro de un inquilino
  - TenantRolesController para gestión de roles y asignación de permisos dentro de un inquilino
  - TenantSectionDefinitionsController para gestión de definiciones de secciones por el Administrador de Inquilino
- La configuración de políticas de autorización basadas en roles, permisos y TenantId:
  - Implementación de TenantAuthorizationHandler y PermissionAuthorizationHandler
  - Aplicación de políticas a los endpoints de la API
  - Verificación de pertenencia al inquilino correcto y posesión de permisos necesarios
- Implementación de un middleware global de manejo de errores:
  - Captura centralizada de excepciones no controladas
  - Transformación de excepciones en respuestas HTTP estructuradas
  - Integración con el sistema de logging
  - Soporte para excepciones de dominio personalizadas
- Implementación de un servicio de serialización para entidades de dominio:
  - Servicio centralizado para manejar la serialización/deserialización segura
  - Configuraciones de seguridad para evitar vulnerabilidades
  - Gestión de referencias circulares y otras consideraciones
  - Documentación detallada de riesgos y mitigaciones
- Implementación de validación de modelos en los controladores:
  - Filtro de acción global (ModelValidationFilter) que utiliza FluentValidation
  - Validación automática y consistente de todos los modelos recibidos en los controladores
  - Integración con IValidationService para reglas de negocio complejas
  - Documentación detallada del enfoque de validación en capas
  - Simplificación del código de los controladores eliminando validaciones manuales repetitivas

### Frontend (En Desarrollo):
- Configuración inicial del proyecto Blazor WebAssembly
- Implementación de CSS personalizado para los componentes UI
- Configuración del HttpClient en Program.cs para la comunicación con la API del backend:
  - Implementación de `AuthTokenHandler` como `DelegatingHandler` para adjuntar automáticamente el token JWT a todas las solicitudes HTTP salientes
  - Configuración de `HttpClientFactory` para utilizar el `AuthTokenHandler`
  - Gestión de errores robusta en las solicitudes HTTP
- Implementación del sistema de autenticación basado en JWT:
  - CustomAuthenticationStateProvider para gestionar tokens JWT
  - Almacenamiento seguro de tokens en localStorage del navegador
  - Integración con el sistema de autorización de Blazor
  - Configuración de rutas protegidas con AuthorizeRouteView
  - Redirección automática al login para usuarios no autenticados
- Implementación del dashboard profesional en la página principal:
  - Tarjetas de estadísticas para usuarios, roles y secciones
  - Sección de actividad reciente
  - Acciones rápidas para operaciones comunes
  - Información del sistema
- Implementación de componentes principales de navegación y autenticación:
  - Layout principal (MainLayout) con visualización condicional basada en estado de autenticación
  - Menú de navegación (NavMenu) con elementos que se muestran/ocultan según rol del usuario
  - Componente de login con validación y manejo de errores
  - Menú de usuario con opción de cierre de sesión
  - Estructura base para visualización dinámica de secciones personalizadas
- Preparación de la estructura para futuras páginas de gestión de usuarios, roles y secciones