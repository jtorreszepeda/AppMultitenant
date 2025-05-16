using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Clase base para los clientes de API que proporciona funcionalidad común
    /// </summary>
    public abstract class ApiClientBase : IApiClient
    {
        protected readonly HttpClient HttpClient;
        protected readonly ILogger Logger;

        /// <summary>
        /// Opciones de serialización JSON predeterminadas
        /// </summary>
        public JsonSerializerOptions JsonOptions { get; }

        /// <summary>
        /// Constructor base para clientes API
        /// </summary>
        /// <param name="httpClient">Cliente HTTP con token de autenticación configurado</param>
        /// <param name="logger">Logger para registrar información y errores</param>
        protected ApiClientBase(HttpClient httpClient, ILogger logger)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            JsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Envía una solicitud GET y deserializa la respuesta
        /// </summary>
        /// <typeparam name="T">Tipo de la respuesta esperada</typeparam>
        /// <param name="endpoint">Endpoint de la API</param>
        /// <returns>Objeto deserializado o default(T) en caso de error</returns>
        protected async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                return await HttpClient.GetFromJsonAsync<T>(endpoint, JsonOptions);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al realizar GET a {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Envía una solicitud GET y deserializa la respuesta, con manejo de posible respuesta vacía
        /// </summary>
        /// <typeparam name="T">Tipo de la respuesta esperada</typeparam>
        /// <param name="endpoint">Endpoint de la API</param>
        /// <returns>Objeto deserializado, default(T) en caso de error o respuesta vacía</returns>
        protected async Task<T> GetAsyncSafe<T>(string endpoint)
        {
            try
            {
                var response = await HttpClient.GetAsync(endpoint);
                
                if (!response.IsSuccessStatusCode)
                {
                    Logger.LogWarning("La solicitud GET a {Endpoint} retornó estado {StatusCode}", 
                        endpoint, response.StatusCode);
                    return default;
                }
                
                if (response.Content.Headers.ContentLength == 0)
                {
                    return default;
                }
                
                return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al realizar GET a {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Envía una solicitud POST con un objeto que será serializado en el cuerpo
        /// </summary>
        /// <typeparam name="TRequest">Tipo del objeto de solicitud</typeparam>
        /// <typeparam name="TResponse">Tipo de la respuesta esperada</typeparam>
        /// <param name="endpoint">Endpoint de la API</param>
        /// <param name="requestData">Datos a enviar en el cuerpo de la solicitud</param>
        /// <returns>Objeto de respuesta deserializado o default(TResponse) en caso de error</returns>
        protected async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest requestData)
        {
            try
            {
                var response = await HttpClient.PostAsJsonAsync(endpoint, requestData, JsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al realizar POST a {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Envía una solicitud POST sin esperar una respuesta tipada
        /// </summary>
        /// <typeparam name="TRequest">Tipo del objeto de solicitud</typeparam>
        /// <param name="endpoint">Endpoint de la API</param>
        /// <param name="requestData">Datos a enviar en el cuerpo de la solicitud</param>
        /// <returns>HttpResponseMessage para que el llamador pueda inspeccionar el resultado</returns>
        protected async Task<HttpResponseMessage> PostAsync<TRequest>(string endpoint, TRequest requestData)
        {
            try
            {
                var response = await HttpClient.PostAsJsonAsync(endpoint, requestData, JsonOptions);
                response.EnsureSuccessStatusCode();
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al realizar POST a {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Envía una solicitud PUT con un objeto que será serializado en el cuerpo
        /// </summary>
        /// <typeparam name="TRequest">Tipo del objeto de solicitud</typeparam>
        /// <typeparam name="TResponse">Tipo de la respuesta esperada</typeparam>
        /// <param name="endpoint">Endpoint de la API</param>
        /// <param name="requestData">Datos a enviar en el cuerpo de la solicitud</param>
        /// <returns>Objeto de respuesta deserializado o default(TResponse) en caso de error</returns>
        protected async Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest requestData)
        {
            try
            {
                var response = await HttpClient.PutAsJsonAsync(endpoint, requestData, JsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al realizar PUT a {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Envía una solicitud PUT sin esperar una respuesta tipada
        /// </summary>
        /// <typeparam name="TRequest">Tipo del objeto de solicitud</typeparam>
        /// <param name="endpoint">Endpoint de la API</param>
        /// <param name="requestData">Datos a enviar en el cuerpo de la solicitud</param>
        /// <returns>HttpResponseMessage para que el llamador pueda inspeccionar el resultado</returns>
        protected async Task<HttpResponseMessage> PutAsync<TRequest>(string endpoint, TRequest requestData)
        {
            try
            {
                var response = await HttpClient.PutAsJsonAsync(endpoint, requestData, JsonOptions);
                response.EnsureSuccessStatusCode();
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al realizar PUT a {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Envía una solicitud DELETE
        /// </summary>
        /// <param name="endpoint">Endpoint de la API</param>
        /// <returns>True si la operación fue exitosa, false en caso contrario</returns>
        protected async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await HttpClient.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al realizar DELETE a {Endpoint}", endpoint);
                throw;
            }
        }
    }
} 