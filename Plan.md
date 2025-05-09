# Plan Detallado para Aplicación Web Multi-Inquilino (Arquitectura Ajustada)

**Meta:** Desarrollar una aplicación web multi-inquilino segura, eficiente y escalable con .NET y Blazor, permitiendo la gestión granular de usuarios, permisos y acceso a secciones por inquilino, siguiendo la arquitectura especificada.

**Principios Guía:**
* **Simplicidad:** Buscar la solución más directa y mantenible.
* **Seguridad:** Priorizar la protección de datos y el acceso seguro.
* **Efectividad:** La aplicación debe cumplir sus objetivos de forma fiable.
* **Eficiencia:** Optimizar el uso de recursos y la velocidad de respuesta.
* **Velocidad:** Tiempos de carga y procesamiento rápidos.
* **Modularidad:** Diseño basado en componentes y módulos cohesivos y poco acoplados.
* **Adherencia a la Arquitectura Especificada:** Cumplir con los lineamientos del documento proporcionado.

---

## 1. Principios Arquitectónicos Clave (Adaptado del Documento Anexo y Multi-Inquilino)

* **Clean Architecture (Backend):** Estricta separación en capas: Dominio, Aplicación, Infraestructura. La Regla de Dependencia es fundamental. El Dominio es el núcleo, la Aplicación orquesta y la Infraestructura implementa detalles externos.
* **Entidades de Dominio como Contrato de Salida de Aplicación (Backend):** Los servicios de la capa de Aplicación en el backend retornarán Entidades de Dominio directamente a sus llamadores (los controladores de API). No habrá DTOs entre la capa de Aplicación y los controladores.
* **API Backend (ASP.NET Core Web API):**
    * Expondrá la funcionalidad de la capa de Aplicación a través de endpoints HTTP.
    * Los controladores recibirán Entidades de Dominio de los servicios de la capa de Aplicación.
    * **Serialización Directa de Entidades de Dominio:** Siguiendo una interpretación estricta de "no DTOs", los controladores API serializarán las Entidades de Dominio directamente para las respuestas HTTP y deserializarán los datos de las solicitudes HTTP directamente a Entidades de Dominio (o tipos primitivos/complejos simples para parámetros de acción).
        * **Advertencia/Riesgo:** Este enfoque, aunque cumple con la restricción, puede llevar a un fuerte acoplamiento entre el contrato de la API y la estructura interna de las Entidades de Dominio. Cambios en las entidades podrían romper la API. También existe el riesgo de exponer accidentalmente más datos de los necesarios o datos sensibles si las entidades contienen información que no debería ser pública. Se debe tener sumo cuidado con la información que contienen las entidades y cómo se serializan.
* **Frontend (Blazor WebAssembly con MVVM):**
    * Se ejecuta en el navegador del cliente.
    * Utilizará el patrón Model-View-ViewModel (MVVM):
        * **Modelo (M):** Corresponde a los datos recibidos de la API Backend, que en este caso serán las representaciones deserializadas de las Entidades de Dominio del backend.
        * **Vista (V):** Componentes Blazor (`.razor` files) que muestran la interfaz de usuario.
        * **ModeloVista (VM - ViewModel):** Clases C# en el proyecto Blazor Wasm. Responsabilidades:
            * Realizar llamadas HTTP (usando `HttpClient`) a la API Backend.
            * Gestionar el estado de la vista.
            * Manejar las Entidades de Dominio (o sus equivalentes del lado del cliente) recibidas de la API y prepararlas para las Vistas (ej. asignándolas a propiedades, gestionando colecciones).
            * Exponer comandos y lógica de presentación para las Vistas.
* **Sin CQRS:** Los servicios de la capa de Aplicación en el backend manejarán tanto consultas (lecturas) como comandos (escrituras/modificaciones).
* **Estrategia Multi-Inquilino:**
    * **Base de Datos Compartida, Esquema Compartido con Discriminador de Inquilino (`TenantId`):** Ofrece un equilibrio entre aislamiento de datos (a nivel de aplicación), costo y complejidad de gestión.
    * **Identificación del Inquilino:** Mediante subdominio (`inquilinoA.miapp.com`), ruta URL o claim en JWT. Se implementará un **Middleware de Resolución de Inquilinos** en ASP.NET Core.
    * **Aislamiento de Datos:** Columna `TenantId` en tablas relevantes y uso de **Entity Framework Core Global Query Filters** para filtrar automáticamente por el `TenantId` actual.


## 2. Estructura del Proyecto (Adaptado del Documento Anexo)

La solución .NET se organizará de la siguiente manera:
NombreAplicacion.sln
│
├── NombreAplicacion.Domain           // Biblioteca de Clases: Entidades, lógica de negocio central, interfaces de dominio.
│   └── Entities
│       ├── Tenant.cs
│       ├── ApplicationUser.cs      // Extiende IdentityUser, con TenantId
│       ├── ApplicationRole.cs      // Extiende IdentityRole, con TenantId
│       ├── Permission.cs
│       ├── AppSectionDefinition.cs
│       ├── FieldDefinition.cs
│       ├── SectionDataEntry.cs
│       └── FieldValue.cs           // O alternativa con JSON
│
├── NombreAplicacion.Application      // Biblioteca de Clases: Casos de uso, interfaces de servicios de aplicación, interfaces de repositorio.
│   ├── Interfaces
│   │   ├── Persistence             // Ej. IUserRepository, ITenantRepository
│   │   └── Services                // Ej. IAuthService, IUserService (retornan Entidades)
│   └── Services                    // Implementaciones de los servicios de aplicación
│       ├── AuthService.cs
│       └── UserService.cs
│
├── NombreAplicacion.Infrastructure   // Biblioteca de Clases: Implementación de interfaces de Aplicación y Dominio.
│   ├── Persistence
│   │   ├── AppDbContext.cs         // DbContext de EF Core, con TenantId y Global Query Filters
│   │   └── Repositories            // Implementaciones de IUserRepository, etc.
│   ├── Identity
│   │   └── TenantResolverService.cs
│   └── ...                         // Otros servicios: email, logging, etc.
│
├── NombreAplicacion.Server           // Proyecto ASP.NET Core Web API (Backend)
│   ├── Controllers                 // Controladores API. Llaman a servicios de Aplicación. Serializan/Deserializan Entidades.
│   │   ├── AuthController.cs
│   │   └── UsersController.cs
│   ├── Middleware
│   │   └── TenantResolutionMiddleware.cs
│   ├── Program.cs                  // Configuración de DI, pipeline HTTP.
│   └── appsettings.json
│
└── NombreAplicacion.Client           // Proyecto Blazor WebAssembly (Frontend)
├── Pages                       // Vistas Blazor (ej. Login.razor, UserManagement.razor)
├── Shared                      // Componentes y Layouts Blazor
├── ViewModels                  // ViewModels para las Vistas (ej. LoginViewModel.cs, UserManagementViewModel.cs)
├── Services                    // Servicios del lado del cliente (ej. para encapsular HttpClient)
│   └── ApiClient.cs            // Servicio para llamadas a la API Backend.
├── wwwroot/
├── _Imports.razor
├── App.razor
└── Program.cs                  // Configuración de DI para Cliente (ViewModels, HttpClient).

## 3. Fases del Plan de Desarrollo (Adaptado del Documento Anexo, con contenido Multi-Inquilino)

### Fase 0: Cimientos y Configuración Inicial
1.  **Definición del Entorno y Herramientas:** IDE (.NET SDK, Git).
2.  **Creación de la Estructura del Proyecto:** Según la sección anterior.
3.  **Configuración Básica de Inyección de Dependencias (DI):** En `Program.cs` de `Server` y `Client`.
4.  **Establecimiento de Logging y Configuración:** Backend y Cliente.
5.  **Configuración Núcleo Multi-Inquilino:**
    * Entidad `Tenant` básica en `NombreAplicacion.Domain`.
    * Configuración inicial de `AppDbContext` para soportar `TenantId`.
    * Implementación básica del `TenantResolutionMiddleware` y `TenantResolverService`.

### Fase 1: Modelado del Dominio (Backend)
1.  **Identificar y Definir Entidades del Dominio:** `Tenant`, `ApplicationUser` (con `TenantId`), `ApplicationRole` (con `TenantId`), `Permission`, `AppSectionDefinition`, `FieldDefinition`, `SectionDataEntry`, `FieldValue` (o alternativa JSON). Incluir lógica de negocio intrínseca y validaciones básicas en las entidades.
2.  **Definir Servicios de Dominio (si son necesarios):** Lógica de negocio compleja que no encaja en una única entidad.
3.  **Definir Interfaces del Dominio (si son necesarias):** Para abstracciones dentro del dominio.

### Fase 2: Núcleo de la Capa de Aplicación (Backend)
1.  **Definir Interfaces de Servicios de Aplicación (Casos de Uso):** Ej. `ITenantAdminService`, `IUserService`, `IAuthService`, `ISectionManagementService`. Estas interfaces definirán métodos que devuelven Entidades de Dominio (o `void` / tipos primitivos).
2.  **Implementar los Servicios de Aplicación:** Lógica que orquesta el dominio y los repositorios. Asegurar que todas las operaciones específicas de inquilino utilicen el `TenantId` resuelto.
3.  **Definir Interfaces de Repositorio:** En la capa de Aplicación (ej. `IUserRepository`, `ITenantRepository`, `ISectionRepository`).

### Fase 3: Implementación de la Infraestructura (Backend)
1.  **Configurar Persistencia de Datos:** `AppDbContext` de Entity Framework Core. Configurar relaciones, Global Query Filters para `TenantId`.
2.  **Implementar Repositorios:** Implementaciones concretas de las interfaces de repositorio, interactuando con EF Core.
3.  **Manejo de Migraciones de Base de Datos:** Para la creación y actualización del esquema.
4.  **Implementar Otros Servicios de Infraestructura:**
    * Servicio de Identidad (ASP.NET Core Identity personalizado con `TenantId`).
    * Implementación completa del `TenantResolverService`.
    * Servicios de logging, correo (si es necesario).

### Fase 4: Desarrollo de la API Backend (`NombreAplicacion.Server`)
1.  **Crear Controladores API:** Para cada conjunto de funcionalidades (ej. `TenantsController`, `UsersController`, `AuthController`, `SectionsController`).
2.  **Inyectar y Utilizar los Servicios de la Capa de Aplicación.**
3.  **Definir Endpoints:** Para operaciones CRUD y otras acciones, siguiendo convenciones RESTful.
4.  **Manejo de Solicitudes y Respuestas:**
    * Los métodos del controlador recibirán solicitudes HTTP.
    * Llamarán a los Servicios de Aplicación, los cuales retornarán Entidades de Dominio.
    * Estas Entidades de Dominio se serializarán directamente en las respuestas HTTP.
    * Los parámetros de entrada de los controladores (si son complejos) provendrán de la deserialización directa del cuerpo de la solicitud a Entidades de Dominio o modelos simples.
    * **Considerar cuidadosamente la validación de entrada** ya que se trabaja directamente con entidades.
5.  **Configurar Autenticación y Autorización para la API:** Usando JWT. Asegurar que la autorización verifique el `TenantId` además de roles/permisos.

### Fase 5: Desarrollo de la Capa de Presentación Cliente (`NombreAplicacion.Client` - Blazor Wasm con MVVM) - Iterativo

Esta fase se repite para cada módulo funcional principal.

* **Módulo de Autenticación y Autorización (por inquilino)**
    1.  **ViewModels:** `LoginViewModel`, `RegisterViewModel`.
        * Llamadas a `/api/auth/login`, `/api/auth/register`.
        * Manejo de tokens JWT en el cliente.
    2.  **Vistas:** `Login.razor`, `Register.razor`.
    3.  **Servicios Cliente:** `AuthApiClient` para encapsular llamadas y manejo de estado de autenticación (`AuthenticationStateProvider` personalizado).

* **Módulo de Gestión de Usuarios (por inquilino)**
    1.  **ViewModels:** `UserListViewModel`, `UserDetailsViewModel`.
        * Llamadas a `/api/tenant/users` (CRUD).
        * Manejo de listas de `ApplicationUser` (entidades).
    2.  **Vistas:** Páginas para listar, crear, editar usuarios.
    3.  **Servicios Cliente:** `UserApiClient`.

* **Módulo de Gestión de Permisos y Roles (por inquilino)**
    1.  **ViewModels:** `RoleListViewModel`, `RolePermissionsViewModel`.
        * Llamadas a `/api/tenant/roles`, `/api/tenant/permissions`.
        * Manejo de `ApplicationRole` y `Permission` (entidades).
    2.  **Vistas:** UI para crear/editar roles y asignar permisos.
    3.  **Servicios Cliente:** `RolePermissionApiClient`.

* **Módulo de Gestión de Secciones de Aplicación (Definición)**
    1.  **ViewModels:** `SectionDefinitionViewModel`.
        * Llamadas a `/api/tenant/sections/definitions`.
        * Manejo de `AppSectionDefinition` y `FieldDefinition` (entidades).
    2.  **Vistas:** UI para diseñar secciones y campos.
    3.  **Servicios Cliente:** `SectionDefinitionApiClient`.

* **Módulo de Contenido Dinámico (CRUD por Sección)**
    1.  **ViewModels:** `SectionDataViewModel` (genérico o por sección).
        * Llamadas a `/api/tenant/sections/{sectionId}/data`.
        * Manejo de `SectionDataEntry` y `FieldValue` (entidades).
    2.  **Vistas:** Componentes para renderizar formularios y tablas dinámicas.
    3.  **Servicios Cliente:** `SectionDataApiClient`.

**Para cada módulo/funcionalidad en esta fase:**
1.  **Análisis de la Funcionalidad del Cliente.**
2.  **Creación de ViewModels:** Inyectar `HttpClient` (o el `ApiClient` encapsulador). Obtener y enviar datos (Entidades de Dominio serializadas). Gestionar estado (`IsLoading`), validación del lado cliente.
3.  **Desarrollo de Vistas/Componentes Blazor:** Enlazar a propiedades y comandos de los ViewModels.
4.  **Configurar `HttpClient` en `Program.cs` del Cliente:** Base URL, handlers de autenticación.

### Fase 6: Estrategia de Pruebas
1.  **Backend:**
    * Pruebas Unitarias del Dominio (lógica de entidades).
    * Pruebas Unitarias de la Aplicación (servicios, mockeando repositorios).
    * Pruebas de Integración de Infraestructura (repositorios con base de datos de prueba).
    * Pruebas de API para los Controladores (usando `WebApplicationFactory` o similar).
2.  **Cliente (Blazor Wasm):**
    * Pruebas Unitarias de ViewModels (mockeando llamadas HTTP/ApiClients).
    * Pruebas de Componentes Blazor (con bUnit).
3.  **Pruebas E2E:** Flujos completos desde UI Blazor hasta backend.

### Fase 7: Aspectos Transversales y Refinamiento
1.  **Seguridad Completa:** Revisión exhaustiva de OWASP Top 10, HTTPS, headers de seguridad, protección de `TenantId`.
2.  **Manejo de Errores Global:** En backend (middleware) y frontend (para mostrar errores amigables).
3.  **Validación Consistente:** Validación en entidades (básica), en servicios de aplicación (reglas de negocio), y en ViewModels (UI).
4.  **Optimización del Rendimiento:** Índices de BD, consultas, caching (si es necesario y dónde), optimización de Blazor Wasm (AOT, lazy loading).
5.  **Revisiones de Código y Refactorización.**

### Fase 8: Despliegue
1.  **Pipeline de CI/CD:** Para `Server` y `Client`.
2.  **Configuración de Entornos:** Dev, Staging, Prod.
3.  **Estrategia de Despliegue.**
4.  **Publicación:** Backend API y Frontend Blazor Wasm (ej. como archivos estáticos o servido por el backend).

### Fase 9: Monitorización y Mantenimiento Continuo
1.  **Monitorización de API Backend y Base de Datos.**
2.  **Recopilación de errores del lado del cliente (opcional).**
3.  **Mantenimiento Regular y Actualizaciones.**
4.  **Iteración y Mejoras basadas en feedback.**



## 4. Pila Tecnológica Detallada (Ajustada)

* **Backend:**
    * **.NET 9 (o LTS más reciente)**
    * **ASP.NET Core Web API**
    * **Entity Framework Core:** ORM, Migrations, Global Query Filters.
    * **ASP.NET Core Identity:** Personalizado para multi-inquilino.
    * **FluentValidation (Recomendado):** Para validaciones robustas en servicios de aplicación o entidades.
    * **Serilog o NLog:** Logging estructurado.
* **Frontend:**
    * **Blazor WebAssembly**
    * **CSS Framework:** MudBlazor.
    * **Gestión de Estado (si es necesario):** Fluxor, Blazor-State, o servicios MVVM bien diseñados.
* **Base de Datos:**
    * PostgreSQL
* **Seguridad Adicional:**
    * HTTPS, Anti-CSRF, Headers de Seguridad, Rate Limiting.
* **Otros:**
    * Redis (Opcional para caching distribuido).



## 5. Patrones de Diseño y Principios Arquitectónicos (Ajustados)

* **Clean Architecture.**
* **MVVM (Model-View-ViewModel) en Blazor:** Como se describió, ViewModels interactúan con servicios que devuelven representaciones de Entidades de Dominio.
* **Repository Pattern.**
* **Unit of Work Pattern** (implícito con EF Core `DbContext`).
* **Dependency Injection (DI).**
* **SOLID Principles.**
* **Middleware (ASP.NET Core).**
* **Options Pattern (ASP.NET Core).**
* **SIN CQRS.**
* **SIN DTOs entre Capa de Aplicación y Controladores API, ni entre Controladores API y Cliente HTTP (Entidades de Dominio serializadas directamente).**



## 6. Consideraciones de Seguridad (Prioridad Alta)

(Se mantienen las mismas consideraciones del plan original, con énfasis en la correcta segregación de datos por `TenantId` en todos los niveles y la validación cuidadosa de las Entidades de Dominio en los límites de la API).

* Autenticación Robusta (MFA para admins).
* Autorización Granular (basada en roles, permisos y `TenantId`).
* Protección contra OWASP Top 10.
* Validación de Entradas (especialmente importante al usar entidades directamente en la API).
* Seguridad de API (HTTPS, Tokens, Rate Limiting).
* Auditoría.
* Gestión de Secretos.



## 7. Consideraciones de Rendimiento (Prioridad Alta)

(Se mantienen las mismas consideraciones del plan original).

* Base de Datos: Indexación (en `TenantId` y columnas de filtro), Consultas Optimizadas, Paginación.
* Caching (con precaución, considerar invalidación por inquilino).
* Blazor WebAssembly: Optimización de Payload (AOT, trimming), Carga Diferida, Virtualización.
* Backend: Operaciones Asíncronas, Response Compression.
* CDN.



## 8. Flujo de Desarrollo y Metodología

(Se mantienen las mismas consideraciones del plan original).

* Metodología Ágil (Scrum/Kanban).
* Control de Versiones (Git).
* CI/CD.
* Pruebas (Unitarias, Integración, E2E, Componentes Blazor).



## 9. Consideraciones para el Futuro y Escalabilidad

(Se mantienen las mismas consideraciones del plan original).

* Escalabilidad de BD.
* Escalabilidad del Backend (monolito modular escalable horizontalmente).
* Posible evolución a servicios más pequeños si es necesario, aunque el objetivo es evitar microservicios.



Este plan ajustado integra los requisitos de la arquitectura proporcionada, manteniendo el enfoque en una aplicación multi-inquilino robusta y siguiendo las mejores prácticas dentro de las restricciones dadas. Recuerda que la decisión de serializar Entidades de Dominio directamente en la API debe manejarse con cuidado para mitigar los riesgos de acoplamiento y exposición de datos.