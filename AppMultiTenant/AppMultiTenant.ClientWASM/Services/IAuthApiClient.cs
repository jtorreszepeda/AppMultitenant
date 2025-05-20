namespace AppMultiTenant.ClientWASM.Services
{
    /// <summary>
    /// Modelo para el request de login
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Correo electrónico del usuario
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para el request de registro
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Nombre de usuario
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico del usuario
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        public string FullName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para la respuesta de autenticación
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// Token JWT de autenticación
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Información básica del usuario
        /// </summary>
        public UserInfo User { get; set; } = new UserInfo();
    }

    /// <summary>
    /// Información básica del usuario
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// Identificador único del usuario
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de usuario
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico del usuario
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        public string FullName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para la solicitud de refresco de token
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// Token JWT actual a refrescar
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interfaz para el servicio cliente que maneja las operaciones de autenticación
    /// </summary>
    public interface IAuthApiClient
    {
        /// <summary>
        /// Inicia sesión con las credenciales proporcionadas
        /// </summary>
        /// <param name="loginRequest">Credenciales de login</param>
        /// <returns>Resultado de la autenticación con token JWT si es exitoso</returns>
        Task<AuthResponse> LoginAsync(LoginRequest loginRequest);

        /// <summary>
        /// Registra un nuevo usuario (si está permitido en la configuración)
        /// </summary>
        /// <param name="registerRequest">Datos de registro</param>
        /// <returns>Resultado del registro</returns>
        Task<AuthResponse> RegisterAsync(RegisterRequest registerRequest);

        /// <summary>
        /// Refresca un token JWT expirado o próximo a expirar
        /// </summary>
        /// <param name="request">Token JWT actual a refrescar</param>
        /// <returns>Nuevo token JWT válido</returns>
        Task<string> RefreshTokenAsync(RefreshTokenRequest request);

        /// <summary>
        /// Cierra la sesión del usuario actual
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task LogoutAsync();

        /// <summary>
        /// Verifica si el usuario está autenticado
        /// </summary>
        /// <returns>True si el usuario está autenticado, false en caso contrario</returns>
        Task<bool> IsAuthenticatedAsync();

        /// <summary>
        /// Obtiene información del usuario actual
        /// </summary>
        /// <returns>Información del usuario autenticado</returns>
        Task<UserInfo> GetCurrentUserAsync();
    }
}