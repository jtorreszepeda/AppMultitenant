using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using AppMultiTenant.Application.Interfaces.Services;
using AppMultiTenant.Application.Configuration;

namespace AppMultiTenant.Infrastructure.Identity
{
    /// <summary>
    /// Implementación básica del servicio de resolución de inquilinos.
    /// Esta versión inicial lee el TenantId de una cabecera HTTP temporalmente.
    /// En versiones futuras, se implementará la resolución por subdominio, ruta o JWT.
    /// </summary>
    public class TenantResolverService : ITenantResolverService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TenantConfiguration _tenantConfiguration;
        private static readonly AsyncLocal<Guid?> _currentTenantId = new AsyncLocal<Guid?>();
        private static readonly AsyncLocal<string?> _currentTenantIdentifier = new AsyncLocal<string?>();

        /// <summary>
        /// Cabecera HTTP utilizada temporalmente para la resolución del inquilino
        /// </summary>
        private const string TENANT_ID_HEADER = "X-TenantId";
        private const string TENANT_IDENTIFIER_HEADER = "X-TenantIdentifier";

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
        /// Obtiene el ID del inquilino actual del contexto HTTP o del valor almacenado en AsyncLocal
        /// </summary>
        /// <returns>GUID del inquilino o null si es una operación de SuperAdmin</returns>
        public Guid? GetCurrentTenantId()
        {
            // Si ya tenemos un valor en el contexto de ejecución actual, lo devolvemos
            if (_currentTenantId.Value.HasValue)
            {
                return _currentTenantId.Value;
            }

            // Intentamos resolver desde el contexto HTTP si está disponible
            if (_httpContextAccessor.HttpContext != null)
            {
                // IMPLEMENTACIÓN TEMPORAL: Leer de cabecera HTTP
                if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(TENANT_ID_HEADER, out var tenantIdHeader))
                {
                    if (Guid.TryParse(tenantIdHeader, out var tenantId))
                    {
                        // Guardamos en el contexto de ejecución para futuras llamadas
                        _currentTenantId.Value = tenantId;
                        return tenantId;
                    }
                }

                // En fases futuras, aquí se implementarán otras estrategias según TenantResolutionStrategy:
                // 1. Resolución por subdominio
                // 2. Resolución por segmento de ruta
                // 3. Resolución por claim de JWT después de autenticación
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

            // Intentamos resolver desde el contexto HTTP si está disponible
            if (_httpContextAccessor.HttpContext != null)
            {
                // IMPLEMENTACIÓN TEMPORAL: Leer de cabecera HTTP
                if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(TENANT_IDENTIFIER_HEADER, out var tenantIdentifierHeader))
                {
                    if (!string.IsNullOrWhiteSpace(tenantIdentifierHeader))
                    {
                        // Guardamos en el contexto de ejecución para futuras llamadas
                        _currentTenantIdentifier.Value = tenantIdentifierHeader;
                        return tenantIdentifierHeader;
                    }
                }

                // En fases futuras, aquí se implementarán otras estrategias de resolución
            }

            // No pudimos resolver un identificador de inquilino
            return null;
        }

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