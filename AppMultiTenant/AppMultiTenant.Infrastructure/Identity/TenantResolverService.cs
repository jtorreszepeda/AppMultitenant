using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using AppMultiTenant.Application.Interfaces.Services;
using AppMultiTenant.Application.Configuration;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading;

namespace AppMultiTenant.Infrastructure.Identity
{
    /// <summary>
    /// Implementación completa del servicio de resolución de inquilinos.
    /// Implementa múltiples estrategias de resolución: subdominio, ruta, cabecera HTTP y claims JWT.
    /// </summary>
    public class TenantResolverService : ITenantResolverService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TenantConfiguration _tenantConfiguration;
        private static readonly AsyncLocal<Guid?> _currentTenantId = new AsyncLocal<Guid?>();
        private static readonly AsyncLocal<string?> _currentTenantIdentifier = new AsyncLocal<string?>();

        // Constantes para las cabeceras HTTP y claims
        private const string TENANT_ID_HEADER = "X-TenantId";
        private const string TENANT_IDENTIFIER_HEADER = "X-TenantIdentifier";
        private const string TENANT_ID_CLAIM = "tenant_id";
        private const string TENANT_IDENTIFIER_CLAIM = "tenant_identifier";

        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        /// <param name="httpContextAccessor">Acceso al contexto HTTP actual</param>
        /// <param name="tenantOptions">Opciones de configuración de inquilinos</param>
        public TenantResolverService(
            IHttpContextAccessor httpContextAccessor,
            IOptions<TenantConfiguration> tenantOptions)
        {
            _httpContextAccessor = httpContextAccessor ?? 
                throw new ArgumentNullException(nameof(httpContextAccessor));
            _tenantConfiguration = tenantOptions?.Value ?? 
                throw new ArgumentNullException(nameof(tenantOptions));
        }

        /// <summary>
        /// Obtiene el ID del inquilino actual usando la estrategia configurada
        /// </summary>
        /// <returns>GUID del inquilino o null si es una operación de SuperAdmin</returns>
        public Guid? GetCurrentTenantId()
        {
            // Si ya tenemos un valor en el contexto de ejecución actual, lo devolvemos
            if (_currentTenantId.Value.HasValue)
            {
                return _currentTenantId.Value;
            }

            // Si el contexto HTTP está disponible, intentamos resolver el tenant
            if (_httpContextAccessor.HttpContext != null)
            {
                // Seleccionamos la estrategia de resolución basada en la configuración
                Guid? tenantId = _tenantConfiguration.TenantResolutionStrategy.ToLowerInvariant() switch
                {
                    "subdomain" => ResolveTenantIdFromSubdomain(),
                    "path" => ResolveTenantIdFromPath(),
                    "header" => ResolveTenantIdFromHeader(),
                    _ => null
                };

                // Si no se pudo resolver por la estrategia primaria, intentamos con JWT claims como fallback
                if (!tenantId.HasValue)
                {
                    tenantId = ResolveTenantIdFromJwtClaims();
                }

                // Si pudimos resolver el tenant, lo guardamos en el contexto y lo devolvemos
                if (tenantId.HasValue)
                {
                    _currentTenantId.Value = tenantId;
                    return tenantId;
                }
            }

            // Si no se pudo resolver y hay un valor predeterminado configurado
            if (!string.IsNullOrEmpty(_tenantConfiguration.DefaultTenantId) && 
                Guid.TryParse(_tenantConfiguration.DefaultTenantId, out var defaultTenantId))
            {
                _currentTenantId.Value = defaultTenantId;
                return defaultTenantId;
            }

            // Si no pudimos resolver un tenant, devolvemos null (modo SuperAdmin)
            return null;
        }

        /// <summary>
        /// Obtiene el identificador (nombre único) del inquilino actual
        /// </summary>
        /// <returns>Identificador del inquilino o null si es SuperAdmin</returns>
        public string? GetCurrentTenantIdentifier()
        {
            // Si ya tenemos un valor en el contexto de ejecución actual, lo devolvemos
            if (_currentTenantIdentifier.Value != null)
            {
                return _currentTenantIdentifier.Value;
            }

            // Si el contexto HTTP está disponible, intentamos resolver el tenant identifier
            if (_httpContextAccessor.HttpContext != null)
            {
                // Seleccionamos la estrategia de resolución basada en la configuración
                string? tenantIdentifier = _tenantConfiguration.TenantResolutionStrategy.ToLowerInvariant() switch
                {
                    "subdomain" => ResolveTenantIdentifierFromSubdomain(),
                    "path" => ResolveTenantIdentifierFromPath(),
                    "header" => ResolveTenantIdentifierFromHeader(),
                    _ => null
                };

                // Si no se pudo resolver por la estrategia primaria, intentamos con JWT claims como fallback
                if (string.IsNullOrEmpty(tenantIdentifier))
                {
                    tenantIdentifier = ResolveTenantIdentifierFromJwtClaims();
                }

                // Si pudimos resolver el tenant identifier, lo guardamos en el contexto y lo devolvemos
                if (!string.IsNullOrEmpty(tenantIdentifier))
                {
                    _currentTenantIdentifier.Value = tenantIdentifier;
                    return tenantIdentifier;
                }
            }

            // No pudimos resolver un identificador de inquilino
            return null;
        }

        #region Métodos de resolución de tenant por subdomain

        /// <summary>
        /// Resuelve el ID del inquilino desde el subdominio de la URL
        /// </summary>
        /// <returns>ID del inquilino o null si no se pudo resolver</returns>
        private Guid? ResolveTenantIdFromSubdomain()
        {
            var host = _httpContextAccessor.HttpContext?.Request.Host.Host;
            if (string.IsNullOrEmpty(host))
                return null;

            // Extraemos el subdominio de la URL
            var subdomain = ExtractSubdomain(host);
            if (string.IsNullOrEmpty(subdomain))
                return null;

            // Aquí debería haber una consulta a la base de datos para obtener el TenantId
            // basado en el subdominio. Por ahora, simplemente verificamos si es una cadena GUID válida.
            if (Guid.TryParse(subdomain, out var tenantId))
                return tenantId;

            // En una implementación real, aquí se buscaría el tenant en la base de datos usando el subdominio
            return null;
        }

        /// <summary>
        /// Resuelve el identificador del inquilino desde el subdominio de la URL
        /// </summary>
        /// <returns>Identificador del inquilino o null si no se pudo resolver</returns>
        private string? ResolveTenantIdentifierFromSubdomain()
        {
            var host = _httpContextAccessor.HttpContext?.Request.Host.Host;
            if (string.IsNullOrEmpty(host))
                return null;

            // El identificador del inquilino es el subdominio
            return ExtractSubdomain(host);
        }

        /// <summary>
        /// Extrae el subdominio de un nombre de host
        /// </summary>
        /// <param name="host">Nombre de host completo (ej. 'tenant.app.com')</param>
        /// <returns>Subdominio o null si no hay subdominio</returns>
        private string? ExtractSubdomain(string host)
        {
            // Ignoramos localhost para desarrollo
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                return null;

            // Dividimos el host por puntos
            var parts = host.Split('.');
            
            // Necesitamos al menos 3 partes para que haya un subdominio (ej. tenant.app.com)
            if (parts.Length < 3)
                return null;

            // El subdominio es la primera parte
            return parts[0];
        }
        
        #endregion

        #region Métodos de resolución de tenant por path

        /// <summary>
        /// Resuelve el ID del inquilino desde el segmento de ruta de la URL
        /// </summary>
        /// <returns>ID del inquilino o null si no se pudo resolver</returns>
        private Guid? ResolveTenantIdFromPath()
        {
            var path = _httpContextAccessor.HttpContext?.Request.Path.Value;
            if (string.IsNullOrEmpty(path))
                return null;

            // Dividimos la ruta por '/'
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            // Si la ruta comienza con 'tenant/', extraemos el siguiente segmento
            if (segments.Length >= 2 && segments[0].Equals("tenant", StringComparison.OrdinalIgnoreCase))
            {
                if (Guid.TryParse(segments[1], out var tenantId))
                    return tenantId;
            }

            // En una implementación real, aquí se buscaría el tenant en la base de datos
            return null;
        }

        /// <summary>
        /// Resuelve el identificador del inquilino desde el segmento de ruta de la URL
        /// </summary>
        /// <returns>Identificador del inquilino o null si no se pudo resolver</returns>
        private string? ResolveTenantIdentifierFromPath()
        {
            var path = _httpContextAccessor.HttpContext?.Request.Path.Value;
            if (string.IsNullOrEmpty(path))
                return null;

            // Dividimos la ruta por '/'
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            // Si la ruta comienza con 'tenant/', extraemos el siguiente segmento
            if (segments.Length >= 2 && segments[0].Equals("tenant", StringComparison.OrdinalIgnoreCase))
            {
                return segments[1];
            }

            return null;
        }
        
        #endregion

        #region Métodos de resolución de tenant por header

        /// <summary>
        /// Resuelve el ID del inquilino desde una cabecera HTTP
        /// </summary>
        /// <returns>ID del inquilino o null si no se pudo resolver</returns>
        private Guid? ResolveTenantIdFromHeader()
        {
            if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue(TENANT_ID_HEADER, out var tenantIdHeader) == true)
            {
                if (Guid.TryParse(tenantIdHeader, out var tenantId))
                {
                    return tenantId;
                }
            }
            return null;
        }

        /// <summary>
        /// Resuelve el identificador del inquilino desde una cabecera HTTP
        /// </summary>
        /// <returns>Identificador del inquilino o null si no se pudo resolver</returns>
        private string? ResolveTenantIdentifierFromHeader()
        {
            if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue(TENANT_IDENTIFIER_HEADER, out var tenantIdentifierHeader) == true)
            {
                if (!string.IsNullOrWhiteSpace(tenantIdentifierHeader))
                {
                    return tenantIdentifierHeader;
                }
            }
            return null;
        }
        
        #endregion

        #region Métodos de resolución de tenant por JWT claims

        /// <summary>
        /// Resuelve el ID del inquilino desde los claims del token JWT en la autenticación
        /// </summary>
        /// <returns>ID del inquilino o null si no se pudo resolver</returns>
        private Guid? ResolveTenantIdFromJwtClaims()
        {
            // Obtenemos el usuario autenticado del contexto HTTP
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            // Buscamos el claim de tenant_id
            var tenantIdClaim = user.FindFirst(TENANT_ID_CLAIM);
            if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            {
                return tenantId;
            }

            return null;
        }

        /// <summary>
        /// Resuelve el identificador del inquilino desde los claims del token JWT en la autenticación
        /// </summary>
        /// <returns>Identificador del inquilino o null si no se pudo resolver</returns>
        private string? ResolveTenantIdentifierFromJwtClaims()
        {
            // Obtenemos el usuario autenticado del contexto HTTP
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            // Buscamos el claim de tenant_identifier
            var tenantIdentifierClaim = user.FindFirst(TENANT_IDENTIFIER_CLAIM);
            if (tenantIdentifierClaim != null && !string.IsNullOrWhiteSpace(tenantIdentifierClaim.Value))
            {
                return tenantIdentifierClaim.Value;
            }

            return null;
        }
        
        #endregion

        /// <summary>
        /// Método estático para establecer el inquilino actual manualmente (para pruebas o contextos especiales)
        /// </summary>
        /// <param name="tenantId">ID del inquilino a establecer</param>
        /// <param name="tenantIdentifier">Identificador del inquilino a establecer</param>
        public static void SetCurrentTenant(Guid? tenantId, string? tenantIdentifier)
        {
            _currentTenantId.Value = tenantId;
            _currentTenantIdentifier.Value = tenantIdentifier;
        }

        /// <summary>
        /// Método estático para limpiar el inquilino actual del contexto
        /// </summary>
        public static void ClearCurrentTenant()
        {
            _currentTenantId.Value = null;
            _currentTenantIdentifier.Value = null;
        }
    }
} 