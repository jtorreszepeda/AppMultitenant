using Microsoft.JSInterop;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Implementación del cliente para llamadas a la API de autenticación
    /// </summary>
    public class AuthApiClient : IAuthApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private static readonly string TokenKey = "authToken";
        private static readonly string UserInfoKey = "userInfo";
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Constructor para el cliente de API de autenticación
        /// </summary>
        /// <param name="httpClient">Cliente HTTP para realizar peticiones</param>
        /// <param name="jsRuntime">Runtime JavaScript para acceder al localStorage</param>
        public AuthApiClient(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Inicia sesión con las credenciales proporcionadas
        /// </summary>
        /// <param name="loginRequest">Credenciales de login</param>
        /// <returns>Resultado de la autenticación con token JWT si es exitoso</returns>
        public async Task<AuthResponse> LoginAsync(LoginRequest loginRequest)
        {
            try
            {
                // Realizar la petición POST al endpoint de login
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);
                
                // Verificar si la respuesta es exitosa
                response.EnsureSuccessStatusCode();
                
                // Deserializar la respuesta
                var result = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);
                
                if (result != null)
                {
                    // Guardar el token en localStorage
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, result.Token);
                    
                    // Guardar la información del usuario en localStorage (para acceso rápido)
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserInfoKey, 
                        JsonSerializer.Serialize(result.User, _jsonOptions));
                    
                    return result;
                }
                
                throw new HttpRequestException("No se pudo procesar la respuesta del servidor");
            }
            catch (HttpRequestException ex)
            {
                // Aquí se podrían manejar diferentes códigos de estado HTTP
                // Por ejemplo, 401 para credenciales inválidas
                throw new HttpRequestException($"Error al iniciar sesión: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Registra un nuevo usuario (si está permitido en la configuración)
        /// </summary>
        /// <param name="registerRequest">Datos de registro</param>
        /// <returns>Resultado del registro</returns>
        public async Task<AuthResponse> RegisterAsync(RegisterRequest registerRequest)
        {
            try
            {
                // Realizar la petición POST al endpoint de registro
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerRequest);
                
                // Verificar si la respuesta es exitosa
                response.EnsureSuccessStatusCode();
                
                // Deserializar la respuesta
                var result = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);
                
                if (result != null)
                {
                    // Guardar el token en localStorage
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, result.Token);
                    
                    // Guardar la información del usuario en localStorage (para acceso rápido)
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserInfoKey, 
                        JsonSerializer.Serialize(result.User, _jsonOptions));
                    
                    return result;
                }
                
                throw new HttpRequestException("No se pudo procesar la respuesta del servidor");
            }
            catch (HttpRequestException ex)
            {
                // Aquí se podrían manejar diferentes códigos de estado HTTP
                throw new HttpRequestException($"Error al registrar usuario: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Refresca un token JWT expirado o próximo a expirar
        /// </summary>
        /// <param name="request">Token JWT actual a refrescar</param>
        /// <returns>Nuevo token JWT válido</returns>
        public async Task<string> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                // Realizar la petición POST al endpoint de refresh-token
                var response = await _httpClient.PostAsJsonAsync("api/auth/refresh-token", request);
                
                // Verificar si la respuesta es exitosa
                response.EnsureSuccessStatusCode();
                
                // Deserializar la respuesta
                var result = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
                var newToken = result.GetProperty("token").GetString();
                
                if (!string.IsNullOrEmpty(newToken))
                {
                    // Actualizar el token en localStorage
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, newToken);
                    return newToken;
                }
                
                throw new HttpRequestException("No se pudo obtener un nuevo token");
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Error al refrescar token: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cierra la sesión del usuario actual
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task LogoutAsync()
        {
            try
            {
                // Llamar al endpoint de logout (si el backend lo requiere)
                await _httpClient.PostAsync("api/auth/logout", null);
            }
            catch (HttpRequestException)
            {
                // Ignorar errores, ya que queremos limpiar el estado local de todas formas
            }
            finally
            {
                // Eliminar el token y la información del usuario del localStorage
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserInfoKey);
            }
        }

        /// <summary>
        /// Verifica si el usuario está autenticado
        /// </summary>
        /// <returns>True si el usuario está autenticado, false en caso contrario</returns>
        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                // Comprobar si hay un token en localStorage
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TokenKey);
                return !string.IsNullOrEmpty(token);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene información del usuario actual
        /// </summary>
        /// <returns>Información del usuario autenticado</returns>
        public async Task<UserInfo> GetCurrentUserAsync()
        {
            try
            {
                // Intentar obtener la información del usuario del localStorage
                var userJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", UserInfoKey);
                
                if (!string.IsNullOrEmpty(userJson))
                {
                    return JsonSerializer.Deserialize<UserInfo>(userJson, _jsonOptions) ?? new UserInfo();
                }
                
                // Si no hay información en localStorage, intentar obtenerla del servidor
                var response = await _httpClient.GetAsync("api/auth/me");
                
                if (response.IsSuccessStatusCode)
                {
                    var userInfo = await response.Content.ReadFromJsonAsync<UserInfo>(_jsonOptions);
                    
                    if (userInfo != null)
                    {
                        // Guardar la información del usuario en localStorage para acceso futuro
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserInfoKey, 
                            JsonSerializer.Serialize(userInfo, _jsonOptions));
                        
                        return userInfo;
                    }
                }
                
                return new UserInfo();
            }
            catch
            {
                return new UserInfo();
            }
        }
    }
} 