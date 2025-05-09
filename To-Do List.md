# Lista de Tareas del Proyecto (To-Do List)
## Aplicación Web MultiTenant

**Versión:** 1.0
**Fecha:** 9 de mayo de 2025
**Referencia:** Plan de Desarrollo (Arquitectura Ajustada) y PRD v1.0

---

## Fase 0: Cimientos y Configuración Inicial

### 0.1. Configuración del Entorno y Proyecto
- [ ] **T0.1.1:** Configurar el entorno de desarrollo (IDE, .NET SDK, Git).
- [x] **T0.1.2:** Crear la estructura de la solución y los proyectos:
    - [x] `AppMultiTenant.Domain` (Biblioteca de Clases)
    - [x] `AppMultiTenant.Application` (Biblioteca de Clases)
    - [x] `AppMultiTenant.Infrastructure` (Biblioteca de Clases)
    - [x] `AppMultiTenant.Server` (ASP.NET Core Web API)
    - [x] `AppMultiTenant.Client` (Blazor WebAssembly)
- [x] **T0.1.3:** Inicializar y configurar el repositorio Git (ej. con GitFlow o similar).
- [ ] **T0.1.4:** Configurar la Inyección de Dependencias (DI) básica en `Program.cs` del proyecto `Server`.
- [ ] **T0.1.5:** Configurar la DI básica en `Program.cs` del proyecto `Client`.
- [ ] **T0.1.6:** Implementar configuración básica de logging (ej. Serilog/NLog) en el proyecto `Server`.
- [ ] **T0.1.7:** Establecer la gestión de configuración (`appsettings.json`, user secrets) para el proyecto `Server`.

### 0.2. Núcleo Multi-Inquilino Básico
- [ ] **T0.2.1:** Definir la entidad `Tenant` básica en `AppMultiTenant.Domain` (`TenantId`, `Name`, `Identifier`, `IsActive`).
- [ ] **T0.2.2:** Configurar `AppDbContext` (EF Core) inicial en `AppMultiTenant.Infrastructure`.
    - [ ] Incluir `DbSet<Tenant>`.
    - [ ] Añadir placeholder para la columna `TenantId` en futuras entidades relevantes.
- [ ] **T0.2.3:** Implementar la interfaz `ITenantResolverService` y una implementación básica inicial en `AppMultiTenant.Infrastructure` (ej. leer `TenantId` de una cabecera HTTP temporalmente).
- [ ] **T0.2.4:** Implementar `TenantResolutionMiddleware` en `AppMultiTenant.Server` para usar `ITenantResolverService` y establecer el contexto del inquilino.
- [ ] **T0.2.5:** Registrar `ITenantResolverService` y `TenantResolutionMiddleware` en `Program.cs` del proyecto `Server`.



## Fase 1: Modelado del Dominio (Backend)

- [ ] **T1.1:** Definir la entidad `ApplicationUser` en `AppMultiTenant.Domain` (extendiendo `Microsoft.AspNetCore.Identity.IdentityUser<Guid>`, agregar `TenantId` y otras propiedades personalizadas).
- [ ] **T1.2:** Definir la entidad `ApplicationRole` en `AppMultiTenant.Domain` (extendiendo `Microsoft.AspNetCore.Identity.IdentityRole<Guid>`, agregar `TenantId` y otras propiedades personalizadas).
- [ ] **T1.3:** Definir la entidad `Permission` en `AppMultiTenant.Domain` (`PermissionId`, `Name`, `Description`).
- [ ] **T1.4:** Definir la entidad de unión `RolePermission` en `AppMultiTenant.Domain` (`RoleId`, `PermissionId`).
- [ ] **T1.5:** Definir la entidad `AppSectionDefinition` en `AppMultiTenant.Domain` (`AppSectionDefinitionId`, `TenantId`, `Name`, `Description`).
- [ ] **T1.6:** Definir la entidad `FieldDefinition` en `AppMultiTenant.Domain` (`FieldDefinitionId`, `AppSectionDefinitionId`, `Name`, `DataType`, `IsRequired`, `ConfigJson` para opciones específicas del tipo).
- [ ] **T1.7:** Definir la entidad `SectionDataEntry` en `AppMultiTenant.Domain` (`SectionDataEntryId`, `AppSectionDefinitionId`, `TenantId`, `CreatedById`, `LastModifiedById`, `CreatedDate`, `LastModifiedDate`).
- [ ] **T1.8:** Definir la entidad `FieldValue` en `AppMultiTenant.Domain` (`FieldValueId`, `SectionDataEntryId`, `FieldDefinitionId`, `ValueString`, `ValueNumber`, `ValueDate`, `ValueBoolean`, `ValueLongString`) O definir una propiedad `DataJson` en `SectionDataEntry` para un enfoque basado en JSON.
- [ ] **T1.9:** Revisar todas las entidades del dominio y agregar validaciones de negocio intrínsecas y lógica si aplica (ej. métodos dentro de la entidad).



## Fase 2: Núcleo de la Capa de Aplicación (Backend)

### 2.1. Definición de Interfaces
- [ ] **T2.1.1:** Definir interfaces de Repositorio en `AppMultiTenant.Application/Interfaces/Persistence`:
    - [ ] `ITenantRepository`
    - [ ] `IUserRepository` (para `ApplicationUser`)
    - [ ] `IRoleRepository` (para `ApplicationRole`)
    - [ ] `IPermissionRepository`
    - [ ] `IAppSectionDefinitionRepository`
    - [ ] `IFieldDefinitionRepository`
    - [ ] `ISectionDataEntryRepository`
    - [ ] `IUnitOfWork` (si se decide explicitarlo sobre el `DbContext` de EF Core)
- [ ] **T2.1.2:** Definir interfaces de Servicios de Aplicación en `AppMultiTenant.Application/Interfaces/Services`:
    - [ ] `IAuthService` (Login, Register, RefreshToken si aplica)
    - [ ] `ISystemAdminTenantService` (Gestión de Inquilinos por SA)
    - [ ] `ITenantUserService` (Gestión de Usuarios por AI)
    - [ ] `ITenantRoleService` (Gestión de Roles y Permisos por AI)
    - [ ] `ITenantSectionDefinitionService` (Gestión de Definiciones de Sección por AI)
    - [ ] `ITenantSectionDataService` (Gestión de Contenido Dinámico por UI/AI)

### 2.2. Implementación de Servicios de Aplicación
- [ ] **T2.2.1:** Implementar `AuthService` en `AppMultiTenant.Application/Services`.
- [ ] **T2.2.2:** Implementar `SystemAdminTenantService`.
- [ ] **T2.2.3:** Implementar `TenantUserService`.
- [ ] **T2.2.4:** Implementar `TenantRoleService`.
- [ ] **T2.2.5:** Implementar `TenantSectionDefinitionService`.
- [ ] **T2.2.6:** Implementar `TenantSectionDataService`.
- [ ] **T2.2.7:** Asegurar que todos los servicios de aplicación que operan en un contexto de inquilino reciban/utilicen el `TenantId` resuelto para la lógica de negocio y la interacción con repositorios.
- [ ] **T2.2.8:** Implementar validación de entradas (ej. con FluentValidation) dentro de los servicios de aplicación para la lógica de negocio.



## Fase 3: Implementación de la Infraestructura (Backend)

### 3.1. Persistencia y Repositorios
- [ ] **T3.1.1:** Configurar completamente `AppDbContext` en `AppMultiTenant.Infrastructure/Persistence`:
    - [ ] Incluir todos los `DbSet` para las entidades del dominio.
    - [ ] Definir todas las relaciones entre entidades (claves foráneas, navegación).
    - [ ] Implementar **Global Query Filters** para `TenantId` en todas las entidades relevantes.
- [ ] **T3.1.2:** Implementar los repositorios concretos en `AppMultiTenant.Infrastructure/Persistence/Repositories`:
    - [ ] `TenantRepository`
    - [ ] `UserRepository`
    - [ ] `RoleRepository`
    - [ ] `PermissionRepository`
    - [ ] `AppSectionDefinitionRepository`
    - [ ] `FieldDefinitionRepository`
    - [ ] `SectionDataEntryRepository`
- [ ] **T3.1.3:** Configurar e inicializar EF Core Migrations.
- [ ] **T3.1.4:** Crear la migración inicial de la base de datos y aplicarla.
- [ ] **T3.1.5:** Implementar `UnitOfWork` (si se definió explícitamente).

### 3.2. Identidad e Infraestructura Adicional
- [ ] **T3.2.1:** Configurar ASP.NET Core Identity en `AppMultiTenant.Infrastructure/Identity` para usar `ApplicationUser`, `ApplicationRole` y `AppDbContext`.
- [ ] **T3.2.2:** Personalizar `UserStore` y `RoleStore` si es necesario para una integración profunda de `TenantId` (si el Global Query Filter no es suficiente para todas las operaciones de Identity).
- [ ] **T3.2.3:** Implementar completamente `TenantResolverService` (ej. resolución por subdominio, luego por claim JWT tras login).
- [ ] **T3.2.4:** Configurar servicios de logging para producción (ej. escribir a un archivo, Application Insights).
- [ ] **T3.2.5:** Registrar todos los servicios de infraestructura y repositorios en `Program.cs` del proyecto `Server` (o en una clase de extensión de DI).



## Fase 4: Desarrollo de la API Backend (`AppMultiTenant.Server`)

- [ ] **T4.1:** Crear `AuthController` con endpoints para Login y Registro de Usuarios (asociados a un inquilino).
- [ ] **T4.2:** Crear `SystemAdminTenantsController` con endpoints CRUD para que el SA gestione inquilinos.
- [ ] **T4.3:** Crear `TenantUsersController` con endpoints CRUD para que el AI gestione usuarios dentro de su inquilino.
- [ ] **T4.4:** Crear `TenantRolesController` con endpoints CRUD para que el AI gestione roles y asigne permisos a roles dentro de su inquilino.
- [ ] **T4.5:** Crear `TenantSectionDefinitionsController` con endpoints CRUD para que el AI gestione definiciones de sección y campos.
- [ ] **T4.6:** Crear `TenantSectionDataController` (genérico o por sección) con endpoints CRUD para gestionar el contenido dinámico.
- [ ] **T4.7:** Implementar generación y validación de JWT para la autenticación de la API.
- [ ] **T4.8:** Configurar políticas de autorización (basadas en roles, permisos y `TenantId`) y aplicarlas a los endpoints.
- [ ] **T4.9:** Implementar un middleware global de manejo de errores para la API.
- [ ] **T4.10:** Asegurar que los controladores serialicen/deserialicen las Entidades de Dominio directamente. Documentar internamente los riesgos y consideraciones de este enfoque.
- [ ] **T4.11:** Implementar validación de modelos en los controladores para las Entidades de Dominio recibidas en las solicitudes.



## Fase 5: Desarrollo de la Capa de Presentación Cliente (`AppMultiTenant.Client` - Blazor Wasm con MVVM)

### 5.1. Configuración General del Cliente Blazor
- [ ] **T5.1.1:** Configurar `HttpClient` en `Program.cs` del cliente (URL base de la API).
- [ ] **T5.1.2:** Implementar `CustomAuthenticationStateProvider` para manejar la autenticación basada en JWT.
- [ ] **T5.1.3:** Crear `MainLayout.razor` y `NavMenu.razor` básicos (con lógica para mostrar/ocultar ítems según rol/autenticación).
- [ ] **T5.1.4:** Implementar un `HttpInterceptor` o `DelegatingHandler` para adjuntar automáticamente el JWT a las solicitudes salientes.
- [ ] **T5.1.5:** Crear servicios `ApiClient` (ej. `AuthApiClient`, `UserApiClient`, etc.) en `AppMultiTenant.Client/Services` para encapsular las llamadas HTTP a la API Backend.

### 5.2. Módulo de Autenticación y Autorización (por inquilino)
- [ ] **T5.2.1:** ViewModel: `LoginViewModel.cs`.
- [ ] **T5.2.2:** ViewModel: `RegisterViewModel.cs` (si el auto-registro está habilitado).
- [ ] **T5.2.3:** Vista: `LoginPage.razor`.
- [ ] **T5.2.4:** Vista: `RegisterPage.razor`.
- [ ] **T5.2.5:** Implementar la lógica en `AuthApiClient` para login/logout y almacenamiento/borrado de tokens.
- [ ] **T5.2.6:** Implementar rutas protegidas y redirección si no está autenticado.

### 5.3. Módulo de Gestión de Usuarios (por AI)
- [ ] **T5.3.1:** ViewModel: `UserListViewModel.cs`.
- [ ] **T5.3.2:** ViewModel: `CreateUserViewModel.cs`.
- [ ] **T5.3.3:** ViewModel: `EditUserViewModel.cs`.
- [ ] **T5.3.4:** Vista: `UserListPage.razor` (con paginación y búsqueda básica).
- [ ] **T5.3.5:** Vista: `CreateUserPage.razor`.
- [ ] **T5.3.6:** Vista: `EditUserPage.razor` (incluir asignación de roles).
- [ ] **T5.3.7:** Implementar métodos CRUD en `UserApiClient`.

### 5.4. Módulo de Gestión de Roles y Permisos (por AI)
- [ ] **T5.4.1:** ViewModel: `RoleListViewModel.cs`.
- [ ] **T5.4.2:** ViewModel: `CreateRoleViewModel.cs`.
- [ ] **T5.4.3:** ViewModel: `EditRoleViewModel.cs` (incluir asignación de permisos del sistema al rol).
- [ ] **T5.4.4:** Vista: `RoleListPage.razor`.
- [ ] **T5.4.5:** Vista: `CreateRolePage.razor`.
- [ ] **T5.4.6:** Vista: `EditRolePage.razor`.
- [ ] **T5.4.7:** Implementar métodos CRUD en `RolePermissionApiClient`.

### 5.5. Módulo de Gestión de Definición de Secciones de Aplicación (por AI)
- [ ] **T5.5.1:** ViewModel: `SectionDefinitionListViewModel.cs`.
- [ ] **T5.5.2:** ViewModel: `CreateSectionDefinitionViewModel.cs`.
- [ ] **T5.5.3:** ViewModel: `EditSectionDefinitionViewModel.cs` (incluir la gestión de `FieldDefinition`).
- [ ] **T5.5.4:** Vista: `SectionDefinitionListPage.razor`.
- [ ] **T5.5.5:** Vista: `CreateSectionDefinitionPage.razor`.
- [ ] **T5.5.6:** Vista: `EditSectionDefinitionPage.razor` (con UI para agregar/editar/eliminar campos dinámicamente).
- [ ] **T5.5.7:** Implementar métodos CRUD en `SectionDefinitionApiClient`.

### 5.6. Módulo de Gestión de Contenido Dinámico (por UI/AI)
- [ ] **T5.6.1:** ViewModel: `SectionDataListViewModel.cs` (genérico, parametrizado por identificador de sección).
- [ ] **T5.6.2:** ViewModel: `CreateSectionDataViewModel.cs` (genérico, dinámico).
- [ ] **T5.6.3:** ViewModel: `EditSectionDataViewModel.cs` (genérico, dinámico).
- [ ] **T5.6.4:** Vista: `SectionDataListPage.razor` (componente genérico para renderizar tabla basada en `AppSectionDefinition`).
- [ ] **T5.6.5:** Vista: `CreateSectionDataPage.razor` (componente genérico para renderizar formulario basado en `AppSectionDefinition`).
- [ ] **T5.6.6:** Vista: `EditSectionDataPage.razor` (componente genérico).
- [ ] **T5.6.7:** Desarrollar componentes Blazor reutilizables para renderizar campos de formulario dinámicos según `FieldDefinition.DataType`.
- [ ] **T5.6.8:** Desarrollar componente Blazor reutilizable para mostrar tablas de datos dinámicas.
- [ ] **T5.6.9:** Implementar métodos CRUD en `SectionDataApiClient`.

### 5.7. Módulo de Administración del Sistema (Super Administrador)
- [ ] **T5.7.1:** ViewModel: `TenantListViewModel_SA.cs`.
- [ ] **T5.7.2:** ViewModel: `CreateTenantViewModel_SA.cs`.
- [ ] **T5.7.3:** ViewModel: `EditTenantViewModel_SA.cs`.
- [ ] **T5.7.4:** Vista: `TenantListPage_SA.razor`.
- [ ] **T5.7.5:** Vista: `CreateTenantPage_SA.razor`.
- [ ] **T5.7.6:** Vista: `EditTenantPage_SA.razor`.
- [ ] **T5.7.7:** Implementar métodos en `SystemAdminApiClient` para la gestión de inquilinos por el SA.
- [ ] **T5.7.8:** Configurar un layout y navegación específicos para la interfaz del Super Administrador si es necesario.



## Fase 6: Estrategia de Pruebas

- [ ] **T6.1:** Configurar proyectos de pruebas unitarias (ej. xUnit/NUnit) para:
    - [ ] `AppMultiTenant.Domain.Tests`
    - [ ] `AppMultiTenant.Application.Tests`
    - [ ] `AppMultiTenant.Client.Tests` (para ViewModels)
- [ ] **T6.2:** Escribir pruebas unitarias para la lógica crítica en entidades del Dominio.
- [ ] **T6.3:** Escribir pruebas unitarias para los Servicios de Aplicación (mockeando repositorios y otros servicios).
- [ ] **T6.4:** Escribir pruebas unitarias para los ViewModels de Blazor (mockeando los ApiClients).
- [ ] **T6.5:** Configurar proyecto de pruebas de integración para `AppMultiTenant.Server.IntegrationTests` (usando `WebApplicationFactory`).
- [ ] **T6.6:** Escribir pruebas de integración para los endpoints de la API, cubriendo flujos de datos y la lógica de `TenantId`.
- [ ] **T6.7:** Escribir pruebas de integración específicas para el `TenantResolutionMiddleware` y el aislamiento de datos por `TenantId`.
- [ ] **T6.8:** Configurar bUnit para pruebas de componentes Blazor en `AppMultiTenant.Client.Tests`.
- [ ] **T6.9:** Escribir pruebas de componentes para los componentes Blazor clave (especialmente los dinámicos y reutilizables).
- [ ] **T6.10 (Opcional):** Investigar y planificar herramientas para pruebas E2E (ej. Playwright, Selenium) para flujos críticos.



## Fase 7: Aspectos Transversales y Refinamiento

- [ ] **T7.1:** Implementar validación de entradas de forma exhaustiva:
    - [ ] En entidades del dominio (DataAnnotations para validaciones simples).
    - [ ] En servicios de aplicación (FluentValidation para lógica de negocio compleja).
    - [ ] En ViewModels/Formularios de Blazor (`EditForm` y DataAnnotations o FluentValidation).
- [ ] **T7.2:** Implementar manejo de errores robusto y feedback al usuario en la UI de Blazor.
- [ ] **T7.3:** Realizar una revisión de seguridad completa y aplicar todas las mitigaciones necesarias de OWASP Top 10.
    - [ ] Configurar HTTPS estrictamente, HSTS, CSP, X-Frame-Options, etc.
    - [ ] Revisar protección contra XSS, CSRF, Inyección.
- [ ] **T7.4:** Realizar revisiones de código periódicas enfocadas en seguridad, rendimiento y adherencia a la arquitectura.
- [ ] **T7.5:** Realizar profiling de rendimiento:
    - [ ] Optimizar consultas a la base de datos (índices, proyecciones).
    - [ ] Optimizar la respuesta de la API.
    - [ ] Optimizar el renderizado y la interactividad de Blazor.
- [ ] **T7.6:** Optimizar el tamaño del payload de Blazor Wasm (AOT compilation, trimming, lazy loading de ensamblados).
- [ ] **T7.7:** Realizar pruebas de usabilidad (UI/UX) y refinar la interfaz basándose en el feedback.
- [ ] **T7.8:** Finalizar la estrategia y configuración de logging para el entorno de producción.
- [ ] **T7.9:** Redactar la documentación para el usuario final (guías para SA, AI, UI).



## Fase 8: Despliegue

- [ ] **T8.1:** Configurar pipelines de Integración Continua (CI) para compilar y probar automáticamente los proyectos `Server` y `Client` en cada commit/PR.
- [ ] **T8.2:** Configurar pipelines de Despliegue Continuo (CD) para desplegar a entornos de Staging y Producción.
- [ ] **T8.3:** Configurar los diferentes entornos (Desarrollo, Staging, Producción) con sus respectivas configuraciones (connection strings, secrets, URLs de API).
- [ ] **T8.4:** Desarrollar y probar scripts o procesos de despliegue.
- [ ] **T8.5:** Establecer y probar la estrategia de migración del esquema de la base de datos como parte del pipeline de CD.
- [ ] **T8.6:** Configurar el alojamiento para la API Backend (ej. Azure App Service, Docker).
- [ ] **T8.7:** Configurar el alojamiento para el cliente Blazor Wasm (ej. Azure Static Web Apps, servido desde el host ASP.NET Core, CDN).
- [ ] **T8.8:** Realizar un despliegue completo de prueba al entorno de Staging.
- [ ] **T8.9:** Planificar y ejecutar el despliegue inicial a Producción.



## Fase 9: Monitorización y Mantenimiento Continuo

- [ ] **T9.1:** Configurar herramientas de monitorización del rendimiento de la aplicación (APM) para el backend (ej. Application Insights).
- [ ] **T9.2:** Configurar la monitorización de la infraestructura (servidores, base de datos).
- [ ] **T9.3 (Opcional):** Implementar seguimiento de errores del lado del cliente para la aplicación Blazor.
- [ ] **T9.4:** Establecer un plan de mantenimiento regular (actualización de dependencias, parches de seguridad).
- [ ] **T9.5:** Planificar y probar el proceso de copia de seguridad y recuperación de desastres de la base de datos.
- [ ] **T9.6:** Establecer un canal para recolectar feedback de los usuarios para futuras mejoras e iteraciones.

---
**Nota:** Esta lista de tareas es exhaustiva y puede requerir ajustes basados en la priorización y los recursos disponibles. Se recomienda usar una herramienta de gestión de proyectos para asignar y rastrear estas tareas.