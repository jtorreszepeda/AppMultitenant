using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using AppMultiTenant.Client.Services;
using Microsoft.Extensions.Logging;

namespace AppMultiTenant.Client.ViewModels
{
    /// <summary>
    /// ViewModel para la funcionalidad de registro de usuarios
    /// </summary>
    public class RegisterViewModel : ViewModelBase
    {
        private readonly IAuthApiClient _authApiClient;
        private readonly NavigationManager _navigationManager;
        private readonly ILogger<RegisterViewModel> _logger;

        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private bool _registrationSuccess;

        /// <summary>
        /// Email del nuevo usuario
        /// </summary>
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del nuevo usuario
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del nuevo usuario
        /// </summary>
        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña para el nuevo usuario
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Confirmación de la contraseña
        /// </summary>
        [Required(ErrorMessage = "La confirmación de contraseña es obligatoria")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Indica si hay un proceso de registro en curso
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Mensaje de error a mostrar en caso de fallo en el registro
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>
        /// Indica si el registro fue exitoso
        /// </summary>
        public bool RegistrationSuccess
        {
            get => _registrationSuccess;
            private set => SetProperty(ref _registrationSuccess, value);
        }

        /// <summary>
        /// Constructor del ViewModel de registro
        /// </summary>
        /// <param name="authApiClient">Cliente de API para autenticación</param>
        /// <param name="navigationManager">Servicio de navegación</param>
        /// <param name="logger">Servicio de logging</param>
        public RegisterViewModel(
            IAuthApiClient authApiClient,
            NavigationManager navigationManager,
            ILogger<RegisterViewModel> logger)
        {
            _authApiClient = authApiClient ?? throw new ArgumentNullException(nameof(authApiClient));
            _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Realiza el registro de un nuevo usuario con los datos ingresados
        /// </summary>
        /// <returns>Verdadero si el registro fue exitoso, falso en caso contrario</returns>
        public async Task<bool> RegisterAsync()
        {
            try
            {
                ErrorMessage = string.Empty;
                RegistrationSuccess = false;
                IsLoading = true;

                var result = await _authApiClient.RegisterAsync(
                    Email, 
                    Password, 
                    ConfirmPassword, 
                    FirstName, 
                    LastName);

                if (result)
                {
                    RegistrationSuccess = true;
                    _logger.LogInformation("Usuario {Email} registrado con éxito", Email);
                    return true;
                }
                else
                {
                    ErrorMessage = "Error al registrar el usuario. Verifica los datos ingresados.";
                    _logger.LogWarning("Registro fallido para {Email}: {Message}", Email, ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error inesperado al procesar el registro";
                _logger.LogError(ex, "Error durante el registro para {Email}", Email);
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Navega a la página de inicio de sesión después de un registro exitoso o si el usuario lo desea
        /// </summary>
        public void NavigateToLogin()
        {
            _navigationManager.NavigateTo("/login");
        }
    }
} 