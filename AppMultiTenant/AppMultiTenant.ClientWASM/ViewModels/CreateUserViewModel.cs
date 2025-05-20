using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using AppMultiTenant.ClientWASM.Services;

namespace AppMultiTenant.ClientWASM.ViewModels
{
    /// <summary>
    /// ViewModel para la creación de usuarios
    /// </summary>
    public class CreateUserViewModel : INotifyPropertyChanged
    {
        private readonly IUserApiClient _userApiClient;
        private readonly IRoleApiClient _roleApiClient;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private List<string> _selectedRoles = new();
        private List<RoleDto> _availableRoles = new();

        public CreateUserViewModel(IUserApiClient userApiClient, IRoleApiClient roleApiClient)
        {
            _userApiClient = userApiClient ?? throw new ArgumentNullException(nameof(userApiClient));
            _roleApiClient = roleApiClient ?? throw new ArgumentNullException(nameof(roleApiClient));
        }

        /// <summary>
        /// Nombre de usuario
        /// </summary>
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        [Display(Name = "Nombre de usuario")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico
        /// </summary>
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña
        /// </summary>
        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Confirmación de contraseña
        /// </summary>
        [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo
        /// </summary>
        [Required(ErrorMessage = "El nombre completo es requerido")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre completo debe tener entre 3 y 100 caracteres")]
        [Display(Name = "Nombre completo")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Indica si se está cargando datos
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Mensaje de error
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>
        /// Mensaje de éxito
        /// </summary>
        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        /// <summary>
        /// Roles disponibles para asignar
        /// </summary>
        public List<RoleDto> AvailableRoles
        {
            get => _availableRoles;
            set => SetProperty(ref _availableRoles, value);
        }

        /// <summary>
        /// IDs de roles seleccionados
        /// </summary>
        public List<string> SelectedRoles
        {
            get => _selectedRoles;
            set => SetProperty(ref _selectedRoles, value);
        }

        /// <summary>
        /// Carga los roles disponibles desde la API
        /// </summary>
        public async Task LoadRolesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var roles = await _roleApiClient.GetAllRolesAsync();
                AvailableRoles = roles.ToList();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar roles: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Envía los datos para crear un nuevo usuario
        /// </summary>
        public async Task<UserDto> CreateUserAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                // Valida el modelo
                if (!IsValid())
                {
                    ErrorMessage = "Hay campos con errores. Por favor, verifica los datos ingresados.";
                    return null;
                }

                // Prepara el modelo para la API
                var createRequest = new CreateUserRequest
                {
                    UserName = UserName,
                    Email = Email,
                    Password = Password,
                    FullName = FullName
                };

                // Crea el usuario
                var createdUser = await _userApiClient.CreateUserAsync(createRequest);

                // Asigna roles si hay seleccionados
                if (SelectedRoles.Count > 0 && createdUser != null)
                {
                    foreach (var roleId in SelectedRoles)
                    {
                        await _userApiClient.AssignRoleToUserAsync(createdUser.Id, roleId);
                    }

                    // Recarga el usuario para obtener los roles actualizados
                    createdUser = await _userApiClient.GetUserByIdAsync(createdUser.Id);
                }

                SuccessMessage = "Usuario creado exitosamente";
                ResetForm();
                return createdUser;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al crear usuario: {ex.Message}";
                return null;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Verifica si el modelo es válido
        /// </summary>
        public bool IsValid()
        {
            var context = new ValidationContext(this);
            var results = new List<ValidationResult>();
            return Validator.TryValidateObject(this, context, results, true);
        }

        /// <summary>
        /// Reinicia el formulario
        /// </summary>
        public void ResetForm()
        {
            UserName = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            FullName = string.Empty;
            SelectedRoles = new List<string>();
        }

        /// <summary>
        /// Limpia los mensajes
        /// </summary>
        public void ClearMessages()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }

        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}