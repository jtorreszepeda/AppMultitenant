using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace AppMultiTenant.ClientWASM.State
{
    /// <summary>
    /// Handler HTTP que agrega automáticamente el token JWT a las solicitudes salientes
    /// </summary>
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly IJSRuntime _jsRuntime;
        private static readonly string TokenKey = "authToken";

        public AuthTokenHandler(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        }

        /// <summary>
        /// Intercepta las solicitudes HTTP y agrega el token JWT al header de autorización
        /// </summary>
        /// <param name="request">Solicitud HTTP a enviar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Respuesta HTTP</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                // Obtener el token del almacenamiento local
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TokenKey, cancellationToken);

                // Si hay un token disponible, agregarlo al header de autorización
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch (InvalidOperationException)
            {
                // Este error ocurre durante el renderizado estático del servidor
                // En este caso, simplemente continuamos sin agregar el token
            }
            catch (Exception ex)
            {
                // Cualquier otro error, registrarlo pero continuar sin romper la cadena
                Console.Error.WriteLine($"Error al agregar token JWT: {ex.Message}");
            }

            // Continuar con la cadena de handlers
            return await base.SendAsync(request, cancellationToken);
        }
    }
} 