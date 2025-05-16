using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Implementación personalizada de AuthenticationStateProvider que maneja la autenticación basada en JWT
    /// </summary>
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<CustomAuthenticationStateProvider> _logger;
        private const string AuthTokenKey = "authToken";
        private const string UserKey = "authUser";

        private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());
        private AuthenticationState _cachedAuthState = new(Anonymous);

        /// <summary>
        /// Constructor del proveedor de estado de autenticación personalizado
        /// </summary>
        /// <param name="jsRuntime">Servicio JS Runtime para acceder al localStorage</param>
        /// <param name="logger">Logger para registro de eventos</param>
        public CustomAuthenticationStateProvider(
            IJSRuntime jsRuntime,
            ILogger<CustomAuthenticationStateProvider> logger)
        {
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el estado de autenticación actual leyendo y validando el token JWT del localStorage
        /// </summary>
        /// <returns>Estado de autenticación actual</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Intenta recuperar el token del localStorage
                var token = await GetTokenAsync();

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogInformation("No se encontró token de autenticación");
                    return new AuthenticationState(Anonymous);
                }

                var identity = GetClaimsIdentity(token);
                if (identity == null)
                {
                    _logger.LogWarning("Token de autenticación inválido");
                    await ClearAuthDataAsync();
                    return new AuthenticationState(Anonymous);
                }

                _logger.LogInformation("Usuario autenticado: {0}", identity.Name);
                var principal = new ClaimsPrincipal(identity);
                _cachedAuthState = new AuthenticationState(principal);
                return _cachedAuthState;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el estado de autenticación");
                return new AuthenticationState(Anonymous);
            }
        }

        /// <summary>
        /// Notifica un cambio en el estado de autenticación al iniciar sesión
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <param name="user">Datos del usuario</param>
        public async Task SetAuthenticationStateAsync(string token, object user)
        {
            if (string.IsNullOrEmpty(token))
            {
                await ClearAuthDataAsync();
                return;
            }

            // Almacena el token y datos de usuario en localStorage
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", AuthTokenKey, token);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserKey, JsonSerializer.Serialize(user));

            // Nota: Ya no es necesario configurar manualmente el HttpClient
            // El AuthTokenHandler se encargará de adjuntar el token a las solicitudes

            // Crea y notifica el nuevo estado de autenticación
            var identity = GetClaimsIdentity(token);
            var principal = new ClaimsPrincipal(identity);
            var authState = new AuthenticationState(principal);
            _cachedAuthState = authState;

            NotifyAuthenticationStateChanged(Task.FromResult(authState));
            _logger.LogInformation("Usuario ha iniciado sesión: {0}", identity.Name);
        }

        /// <summary>
        /// Cierra la sesión del usuario actual
        /// </summary>
        public async Task LogoutAsync()
        {
            await ClearAuthDataAsync();

            // Nota: Ya no es necesario limpiar manualmente el HttpClient
            // El AuthTokenHandler se encargará de no adjuntar ningún token al localStorage estar vacío

            // Notifica la salida
            _cachedAuthState = new AuthenticationState(Anonymous);
            NotifyAuthenticationStateChanged(Task.FromResult(_cachedAuthState));
            _logger.LogInformation("Usuario ha cerrado sesión");
        }

        /// <summary>
        /// Obtiene los claims de un token JWT
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <returns>ClaimsIdentity basada en los claims del token</returns>
        private ClaimsIdentity GetClaimsIdentity(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                // Verifica si el token ha expirado
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    _logger.LogWarning("Token JWT expirado");
                    return null;
                }

                var claims = jwtToken.Claims.ToList();

                // Asegura que tengamos un claim para el nombre (preferiblemente email o nombre de usuario)
                var nameClaim = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)
                             ?? claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)
                             ?? claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)
                             ?? claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);

                if (nameClaim != null)
                {
                    // Agrega el claim de nombre si no existe
                    if (!claims.Any(c => c.Type == ClaimTypes.Name))
                    {
                        claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value));
                    }
                }

                // Agrega roles si están presentes en el token
                var roleClaims = claims.Where(c => c.Type == "role" || c.Type == ClaimTypes.Role);

                return new ClaimsIdentity(claims, "jwt");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar el token JWT");
                return null;
            }
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
                _logger.LogError(ex, "Error al obtener el token del localStorage");
                return null;
            }
        }

        /// <summary>
        /// Obtiene los datos del usuario desde el localStorage
        /// </summary>
        /// <typeparam name="T">Tipo para deserializar los datos del usuario</typeparam>
        /// <returns>Datos del usuario o default(T) si no existen</returns>
        public async Task<T> GetUserDataAsync<T>()
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", UserKey);
                if (string.IsNullOrEmpty(json))
                    return default;

                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los datos del usuario del localStorage");
                return default;
            }
        }

        /// <summary>
        /// Limpia los datos de autenticación del localStorage
        /// </summary>
        private async Task ClearAuthDataAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", AuthTokenKey);
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar los datos de autenticación");
            }
        }
    }
}