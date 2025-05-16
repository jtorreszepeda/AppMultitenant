using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using AppMultiTenant.Client.Services;
using Microsoft.Extensions.Logging;

namespace AppMultiTenant.Client.ViewModels
{
    /// <summary>
    /// ViewModel para la funcionalidad de inicio de sesión
    /// </summary>
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthApiClient _authApiClient;
        private readonly NavigationManager _navigationManager;
        private readonly ILogger<LoginViewModel> _logger;

        private bool _isLoading;
        private string _errorMessage = string.Empty;

        /// <summary>
        /// Email o nombre de usuario para iniciar sesión
        /// </summary>
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña para iniciar sesión
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Recordar credenciales para próximos inicios de sesión
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        /// Indica si hay un proceso de inicio de sesión en curso
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Mensaje de error a mostrar en caso de fallo en el inicio de sesión
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>
        /// Constructor del ViewModel de inicio de sesión
        /// </summary>
        /// <param name="authApiClient">Cliente de API para autenticación</param>
        /// <param name="navigationManager">Servicio de navegación</param>
        /// <param name="logger">Servicio de logging</param>
        public LoginViewModel(
            IAuthApiClient authApiClient,
            NavigationManager navigationManager,
            ILogger<LoginViewModel> logger)
        {
            _authApiClient = authApiClient ?? throw new ArgumentNullException(nameof(authApiClient));
            _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Realiza el inicio de sesión con las credenciales ingresadas
        /// </summary>
        /// <param name="returnUrl">URL opcional a la que redirigir después del inicio de sesión exitoso</param>
        /// <returns>Verdadero si el inicio de sesión fue exitoso, falso en caso contrario</returns>
        public async Task<bool> LoginAsync(string returnUrl = null)
        {
            try
            {
                ErrorMessage = string.Empty;
                IsLoading = true;

                var authResponse = await _authApiClient.LoginAsync(Email, Password);

                if (authResponse?.IsSuccess == true)
                {
                    _logger.LogInformation("Usuario {Email} autenticado con éxito", Email);
                    
                    // Navegar a la URL de retorno después del inicio de sesión exitoso o a la página principal si no hay URL de retorno
                    _navigationManager.NavigateTo(!string.IsNullOrEmpty(returnUrl) ? returnUrl : "/");
                    return true;
                }
                else
                {
                    ErrorMessage = authResponse?.ErrorMessage ?? "Error al iniciar sesión. Verifique sus credenciales.";
                    _logger.LogWarning("Inicio de sesión fallido para {Email}: {Message}", Email, ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error inesperado al procesar el inicio de sesión";
                _logger.LogError(ex, "Error durante el inicio de sesión para {Email}", Email);
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Navega a la página de registro de usuario (si está habilitada)
        /// </summary>
        /// <param name="returnUrl">URL opcional a la que redirigir después del registro exitoso</param>
        public void NavigateToRegister(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                _navigationManager.NavigateTo($"/register?returnUrl={Uri.EscapeDataString(returnUrl)}");
            }
            else
            {
                _navigationManager.NavigateTo("/register");
            }
        }
    }
} 