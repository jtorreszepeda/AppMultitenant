using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace AppMultiTenant.Client.State
{
    /// <summary>
    /// Proveedor personalizado de estado de autenticación para manejar tokens JWT
    /// </summary>
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private static readonly string TokenKey = "authToken";
        private static readonly string UserDataKey = "userData";
        private AuthenticationState _anonymous => new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        public CustomAuthenticationStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        }

        /// <summary>
        /// Obtiene el estado de autenticación actual
        /// </summary>
        /// <returns>El estado de autenticación basado en el token JWT almacenado</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Verificar si podemos acceder a JavaScript
                // Durante el renderizado estático del servidor, debemos retornar un estado anónimo
                var isAuthenticated = false;
                var token = string.Empty;

                try
                {
                    // Obtener el token del almacenamiento local
                    token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TokenKey);
                    isAuthenticated = !string.IsNullOrEmpty(token);
                }
                catch (InvalidOperationException)
                {
                    // Esta excepción ocurre durante el renderizado del servidor
                    // Simplemente devolvemos un usuario anónimo
                    return _anonymous;
                }

                if (!isAuthenticated)
                {
                    return _anonymous;
                }

                // Crear el estado de autenticación basado en las claims del token
                var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
                return new AuthenticationState(authenticatedUser);
            }
            catch (Exception)
            {
                // En caso de error, eliminar el token y devolver usuario anónimo
                try
                {
                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserDataKey);
                }
                catch
                {
                    // Ignorar errores al intentar eliminar del localStorage durante renderizado servidor
                }

                return _anonymous;
            }
        }

        /// <summary>
        /// Marca al usuario como autenticado y configura el token JWT
        /// </summary>
        /// <param name="token">Token JWT recibido del servidor</param>
        /// <param name="userData">Datos adicionales del usuario (opcional)</param>
        public async Task MarkUserAsAuthenticated(string token, object userData = null)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
                
                if (userData != null)
                {
                    var userDataJson = System.Text.Json.JsonSerializer.Serialize(userData);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserDataKey, userDataJson);
                }
            }
            catch (InvalidOperationException)
            {
                // Ignorar durante el renderizado estático
            }

            // Crear nuevo estado de autenticación y notificar a los componentes
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
            var authState = new AuthenticationState(authenticatedUser);
            
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        /// <summary>
        /// Marca al usuario como desconectado eliminando su token
        /// </summary>
        public async Task MarkUserAsLoggedOut()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserDataKey);
            }
            catch (InvalidOperationException)
            {
                // Ignorar durante el renderizado estático
            }
            
            // Notificar cambio de estado a anónimo
            NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
        }

        /// <summary>
        /// Obtiene los datos del usuario almacenados
        /// </summary>
        /// <typeparam name="T">Tipo del objeto de datos de usuario</typeparam>
        /// <returns>Datos del usuario o default(T) si no hay datos</returns>
        public async Task<T> GetUserDataAsync<T>()
        {
            try
            {
                var userDataJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", UserDataKey);
                
                if (string.IsNullOrEmpty(userDataJson))
                {
                    return default;
                }

                try
                {
                    return System.Text.Json.JsonSerializer.Deserialize<T>(userDataJson);
                }
                catch
                {
                    return default;
                }
            }
            catch (InvalidOperationException)
            {
                // Durante el renderizado estático
                return default;
            }
        }

        /// <summary>
        /// Extrae las claims de un token JWT
        /// </summary>
        /// <param name="jwt">Token JWT</param>
        /// <returns>Colección de claims del token</returns>
        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            return token.Claims;
        }
    }
} 