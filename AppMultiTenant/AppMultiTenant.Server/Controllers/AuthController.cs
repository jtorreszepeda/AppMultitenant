using AppMultiTenant.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppMultiTenant.Server.Controllers
{
    /// <summary>
    /// Controlador para gestionar operaciones de autenticación y registro de usuarios
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidationService _validationService;

        /// <summary>
        /// Constructor del controlador de autenticación
        /// </summary>
        /// <param name="authService">Servicio de autenticación</param>
        /// <param name="validationService">Servicio de validación</param>
        public AuthController(IAuthService authService, IValidationService validationService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        }

        /// <summary>
        /// Autentica un usuario y devuelve un token JWT
        /// </summary>
        /// <param name="model">Modelo con credenciales de usuario</param>
        /// <returns>Token JWT y datos básicos del usuario</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (token, user) = await _authService.LoginAsync(model.Email, model.Password);

            if (token == null || user == null)
            {
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    userName = user.UserName,
                    email = user.Email,
                    fullName = user.FullName
                }
            });
        }

        /// <summary>
        /// Registra un nuevo usuario asociado al inquilino actual
        /// </summary>
        /// <param name="model">Datos del usuario a registrar</param>
        /// <returns>Usuario creado y token JWT</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var (user, token) = await _authService.RegisterUserAsync(
                    model.UserName,
                    model.Email,
                    model.Password,
                    model.FullName);

                return Ok(new
                {
                    token,
                    user = new
                    {
                        id = user.Id,
                        userName = user.UserName,
                        email = user.Email,
                        fullName = user.FullName
                    }
                });
            }
            catch (Exception ex)
            {
                // Considera manejar excepciones específicas (ej. duplicado de usuario)
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cierra la sesión de un usuario
        /// </summary>
        /// <returns>Confirmación de cierre de sesión</returns>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Obtener el ID del usuario del token JWT (se implementará en una tarea futura)
            // Por ahora, simplemente devolvemos una confirmación
            return Ok(new { message = "Sesión cerrada correctamente" });
        }
    }

    /// <summary>
    /// Modelo de solicitud para el inicio de sesión
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Correo electrónico del usuario
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo de solicitud para el registro de usuario
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Nombre de usuario
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico del usuario
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MinLength(6)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        public string FullName { get; set; } = string.Empty;
    }
}