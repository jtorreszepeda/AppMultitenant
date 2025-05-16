using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// DelegatingHandler personalizado que adjunta automáticamente el token JWT a las solicitudes HTTP salientes
    /// </summary>
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<AuthTokenHandler> _logger;
        private const string AuthTokenKey = "authToken";

        /// <summary>
        /// Constructor para el manejador de token de autenticación
        /// </summary>
        /// <param name="jsRuntime">Servicio JS Runtime para acceder al localStorage</param>
        /// <param name="logger">Logger para registro de eventos</param>
        public AuthTokenHandler(IJSRuntime jsRuntime, ILogger<AuthTokenHandler> logger)
        {
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        /// <summary>
        /// Intercepta las solicitudes HTTP salientes y añade el token JWT si está disponible
        /// </summary>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                // Obtener el token JWT del localStorage
                var token = await GetTokenAsync();

                // Si hay un token disponible, añadirlo como cabecera de autorización
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    _logger.LogDebug("Token JWT adjuntado a la solicitud a {RequestUri}", request.RequestUri);
                }
                else
                {
                    _logger.LogDebug("No se encontró token JWT para adjuntar a la solicitud a {RequestUri}", request.RequestUri);
                }
            }
            catch (Exception ex)
            {
                // Registrar la excepción pero no interrumpir la solicitud
                _logger.LogError(ex, "Error al intentar adjuntar el token JWT a la solicitud");
            }

            // Continuar con la cadena de handlers
            return await base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Obtiene el token JWT del localStorage
        /// </summary>
        /// <returns>Token JWT o null si no existe</returns>
        private async Task<string> GetTokenAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", AuthTokenKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el token JWT del localStorage");
                return null;
            }
        }
    }
}