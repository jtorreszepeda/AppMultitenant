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

4. Aplicar migraciones de Entity Framework Core
```
cd AppMultiTenant.Server
dotnet ef database update
```

5. Ejecutar el proyecto
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