namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Interfaz para el cliente de API de autenticación
    /// </summary>
    public interface IAuthApiClient : IApiClient
    {
        /// <summary>
        /// Realiza el inicio de sesión de un usuario
        /// </summary>
        /// <param name="email">Correo electrónico del usuario</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <returns>Token JWT y datos del usuario en caso de éxito, null en caso contrario</returns>
        Task<AuthResponse> LoginAsync(string email, string password);

        /// <summary>
        /// Registra un nuevo usuario (si está permitido)
        /// </summary>
        /// <param name="email">Correo electrónico del usuario</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <param name="confirmPassword">Confirmación de contraseña</param>
        /// <param name="firstName">Nombre del usuario</param>
        /// <param name="lastName">Apellido del usuario</param>
        /// <returns>True si el registro fue exitoso, false en caso contrario</returns>
        Task<bool> RegisterAsync(string email, string password, string confirmPassword, string firstName, string lastName);
    }

    /// <summary>
    /// Clase que representa la respuesta de autenticación
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// Indica si la autenticación fue exitosa
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Token JWT para autorización
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Fecha de caducidad del token
        /// </summary>
        public DateTime Expiration { get; set; }

        /// <summary>
        /// Datos básicos del usuario autenticado
        /// </summary>
        public UserAuthData User { get; set; }

        /// <summary>
        /// Mensaje de error en caso de fallo
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Clase que representa los datos básicos del usuario autenticado
    /// </summary>
    public class UserAuthData
    {
        /// <summary>
        /// Identificador del usuario
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Correo electrónico del usuario
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Roles asignados al usuario
        /// </summary>
        public IEnumerable<string> Roles { get; set; }
    }
} 