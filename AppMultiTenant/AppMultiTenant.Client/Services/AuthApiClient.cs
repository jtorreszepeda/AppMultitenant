using System.Net.Http.Json;

namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Implementación del cliente de API de autenticación
    /// </summary>
    public class AuthApiClient : ApiClientBase, IAuthApiClient
    {
        private readonly CustomAuthenticationStateProvider _authStateProvider;

        /// <summary>
        /// Constructor para el cliente de autenticación
        /// </summary>
        /// <param name="httpClient">Cliente HTTP configurado</param>
        /// <param name="logger">Servicio de logging</param>
        /// <param name="authStateProvider">Proveedor de estado de autenticación</param>
        public AuthApiClient(
            HttpClient httpClient,
            ILogger<AuthApiClient> logger,
            CustomAuthenticationStateProvider authStateProvider)
            : base(httpClient, logger)
        {
            _authStateProvider = authStateProvider ?? throw new ArgumentNullException(nameof(authStateProvider));
        }

        /// <summary>
        /// Realiza el inicio de sesión de un usuario
        /// </summary>
        /// <param name="email">Correo electrónico del usuario</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <returns>Token JWT y datos del usuario en caso de éxito, null en caso contrario</returns>
        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            try
            {
                var loginRequest = new
                {
                    Email = email,
                    Password = password
                };

                var response = await HttpClient.PostAsJsonAsync("/api/auth/login", loginRequest, JsonOptions);
                
                if (!response.IsSuccessStatusCode)
                {
                    Logger.LogWarning("Inicio de sesión fallido para {Email}. Código: {StatusCode}", 
                        email, response.StatusCode);
                    
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Error de inicio de sesión: {response.StatusCode}"
                    };
                }

                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
                
                if (authResponse?.IsSuccess == true && !string.IsNullOrEmpty(authResponse.Token))
                {
                    // Actualizar el estado de autenticación (almacena el token en localStorage)
                    await _authStateProvider.SetAuthenticationStateAsync(authResponse.Token, authResponse.User);
                    Logger.LogInformation("Usuario {Email} autenticado con éxito", email);
                }
                else
                {
                    Logger.LogWarning("Respuesta de inicio de sesión inválida para {Email}", email);
                }

                return authResponse;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error durante el proceso de inicio de sesión para {Email}", email);
                return new AuthResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Error al procesar la solicitud de inicio de sesión"
                };
            }
        }

        /// <summary>
        /// Registra un nuevo usuario (si está permitido)
        /// </summary>
        /// <param name="email">Correo electrónico del usuario</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <param name="confirmPassword">Confirmación de contraseña</param>
        /// <param name="firstName">Nombre del usuario</param>
        /// <param name="lastName">Apellido del usuario</param>
        /// <returns>True si el registro fue exitoso, false en caso contrario</returns>
        public async Task<bool> RegisterAsync(string email, string password, string confirmPassword, string firstName, string lastName)
        {
            try
            {
                var registerRequest = new
                {
                    Email = email,
                    Password = password,
                    ConfirmPassword = confirmPassword,
                    FirstName = firstName,
                    LastName = lastName
                };

                var response = await HttpClient.PostAsJsonAsync("/api/auth/register", registerRequest, JsonOptions);
                
                if (!response.IsSuccessStatusCode)
                {
                    Logger.LogWarning("Registro fallido para {Email}. Código: {StatusCode}", 
                        email, response.StatusCode);
                    return false;
                }

                Logger.LogInformation("Usuario {Email} registrado con éxito", email);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error durante el proceso de registro para {Email}", email);
                return false;
            }
        }

        /// <summary>
        /// Cierra la sesión del usuario actual
        /// </summary>
        /// <returns>Tarea que representa la operación asincrónica</returns>
        public async Task LogoutAsync()
        {
            await _authStateProvider.LogoutAsync();
            Logger.LogInformation("Cierre de sesión realizado con éxito");
        }

        /// <summary>
        /// Verifica si el usuario está autenticado actualmente
        /// </summary>
        /// <returns>True si hay un token válido almacenado, false en caso contrario</returns>
        public async Task<bool> IsAuthenticatedAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            return authState.User.Identity?.IsAuthenticated == true;
        }

        /// <summary>
        /// Obtiene los datos del usuario autenticado actualmente
        /// </summary>
        /// <returns>Datos del usuario autenticado o null si no hay sesión</returns>
        public async Task<UserAuthData> GetCurrentUserAsync()
        {
            return await _authStateProvider.GetUserDataAsync<UserAuthData>();
        }

        /// <summary>
        /// Refresca el token de autenticación si está próximo a expirar
        /// </summary>
        /// <returns>True si el token fue refrescado con éxito, false si no fue necesario o falló</returns>
        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                // Solo intentar refrescar si el usuario está autenticado
                if (!await IsAuthenticatedAsync())
                {
                    return false;
                }

                var response = await HttpClient.PostAsync("/api/auth/refresh-token", null);
                
                if (!response.IsSuccessStatusCode)
                {
                    Logger.LogWarning("Falló el refresco del token. Código: {StatusCode}", response.StatusCode);
                    return false;
                }

                var refreshResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
                
                if (refreshResponse?.IsSuccess == true && !string.IsNullOrEmpty(refreshResponse.Token))
                {
                    // Actualizar el estado de autenticación con el nuevo token
                    await _authStateProvider.SetAuthenticationStateAsync(refreshResponse.Token, refreshResponse.User);
                    Logger.LogInformation("Token refrescado con éxito");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error durante el proceso de refresco del token");
                return false;
            }
        }
    }
} 