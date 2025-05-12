# Configuración del Proyecto Server

Este documento describe la estructura de configuración para el proyecto `AppMultiTenant.Server`, incluyendo archivos de configuración, user secrets y variables de entorno.

## Archivos de Configuración

El proyecto utiliza los siguientes archivos de configuración:

- **appsettings.json**: Configuración global predeterminada
- **appsettings.Development.json**: Configuración específica para entorno de desarrollo (sobrescribe valores de appsettings.json)
- **User Secrets**: Almacenamiento seguro para valores sensibles (solo en desarrollo)
- **Variables de entorno**: Pueden utilizarse para sobrescribir cualquier configuración en producción

## Secciones de Configuración Principales

### ConnectionStrings

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=AppMultiTenant;Username=postgres;Password=YourStrongPasswordHere"
}
```

Contiene las cadenas de conexión a la base de datos. En producción, la contraseña debe almacenarse en User Secrets o variables de entorno.

### TenantConfiguration

```json
"TenantConfiguration": {
  "TenantResolutionStrategy": "Subdomain",
  "DefaultTenantId": "00000000-0000-0000-0000-000000000000",
  "DevModeEnabled": true
}
```

Configuración relacionada con la estrategia multi-inquilino:

- **TenantResolutionStrategy**: Método para identificar el inquilino ("Subdomain", "Path", "Header")
- **DefaultTenantId**: ID del inquilino predeterminado
- **DevModeEnabled**: Habilita características específicas de desarrollo (solo en desarrollo)

### JwtSettings

```json
"JwtSettings": {
  "Issuer": "AppMultiTenant",
  "Audience": "AppMultiTenant.Client",
  "ExpiryInMinutes": 60,
  "RefreshTokenExpiryInDays": 7,
  "SecretKey": "your_secret_key_here" // ¡NO incluir en control de versiones!
}
```

Configuración para tokens JWT:

- **SecretKey**: Clave para firmar tokens JWT (debe almacenarse en User Secrets o variables de entorno)
- **Issuer y Audience**: Identificadores para el emisor y audiencia de los tokens
- **ExpiryInMinutes y RefreshTokenExpiryInDays**: Tiempos de caducidad

## User Secrets (Solo Desarrollo)

En desarrollo, se utilizan User Secrets para almacenar información sensible. Para configurar:

1. Los User Secrets ya están inicializados para este proyecto con el ID: `AppMultiTenant-Server-DE05E6D4-E5FC-4B1B-93A4-91D7057465EA`

2. Para establecer valores:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=AppMultiTenant_Dev;Username=postgres;Password=your_dev_password"
dotnet user-secrets set "JwtSettings:SecretKey" "your_super_secure_jwt_signature_key_for_development"
```

3. Para listar todos los secrets configurados:

```powershell
dotnet user-secrets list
```

## Variables de Entorno (Producción)

En producción, se recomienda utilizar variables de entorno para sobrescribir configuraciones sensibles.

Ejemplos:

- `ConnectionStrings__DefaultConnection`: Cadena de conexión a la base de datos
- `JwtSettings__SecretKey`: Clave secreta para JWT
- `TenantConfiguration__TenantResolutionStrategy`: Estrategia de resolución de inquilinos

## Jerarquía de Configuración

La aplicación carga la configuración en el siguiente orden (posterior sobrescribe anterior):

1. appsettings.json
2. appsettings.{Environment}.json
3. User Secrets (solo en Development)
4. Variables de entorno 