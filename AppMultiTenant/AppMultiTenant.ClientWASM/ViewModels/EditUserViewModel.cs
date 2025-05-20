using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using AppMultiTenant.ClientWASM.Services;

namespace AppMultiTenant.ClientWASM.ViewModels
{
    /// <summary>
    /// ViewModel para la edición de usuarios
    /// </summary>
    public class EditUserViewModel : INotifyPropertyChanged
    {
        private readonly IUserApiClient _userApiClient;
        private readonly IRoleApiClient _roleApiClient;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private List<string> _selectedRoles = new();
        private List<RoleDto> _availableRoles = new();
        private string _userId = string.Empty;
        private bool _isActive = true;

        public EditUserViewModel(IUserApiClient userApiClient, IRoleApiClient roleApiClient)
        {
            _userApiClient = userApiClient ?? throw new ArgumentNullException(nameof(userApiClient));
            _roleApiClient = roleApiClient ?? throw new ArgumentNullException(nameof(roleApiClient));
        }

        /// <summary>
        /// ID del usuario que se está editando
        /// </summary>
        public string UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
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
        /// Nombre completo
        /// </summary>
        [Required(ErrorMessage = "El nombre completo es requerido")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre completo debe tener entre 3 y 100 caracteres")]
        [Display(Name = "Nombre completo")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el usuario está activo
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        /// <summary>
        /// Nueva contraseña (opcional al editar)
        /// </summary>
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña (dejar en blanco para no cambiarla)")]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirmación de nueva contraseña
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmNewPassword { get; set; } = string.Empty;

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
        /// Carga los datos del usuario por su ID
        /// </summary>
        /// <param name="userId">ID del usuario a cargar</param>
        public async Task LoadUserAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    ErrorMessage = "ID de usuario no especificado";
                    return;
                }

                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;
                UserId = userId;

                // Cargar datos del usuario
                var user = await _userApiClient.GetUserByIdAsync(userId);

                // Actualizar propiedades del ViewModel
                UserName = user.UserName;
                Email = user.Email;
                FullName = user.FullName;
                IsActive = user.IsActive;

                // Limpiar campos de contraseña
                NewPassword = string.Empty;
                ConfirmNewPassword = string.Empty;

                // Cargar roles seleccionados
                SelectedRoles = user.Roles.ToList();

                // Cargar todos los roles disponibles
                await LoadRolesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar datos del usuario: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Carga los roles disponibles desde la API
        /// </summary>
        public async Task LoadRolesAsync()
        {
            try
            {
                var roles = await _roleApiClient.GetAllRolesAsync();
                AvailableRoles = roles.ToList();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar roles: {ex.Message}";
            }
        }

        /// <summary>
        /// Actualiza los datos del usuario
        /// </summary>
        public async Task<UserDto> UpdateUserAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                // Validar modelo
                if (!IsValid())
                {
                    ErrorMessage = "Hay campos con errores. Por favor, verifica los datos ingresados.";
                    return null;
                }

                // Preparar modelo para actualización
                var updateRequest = new UpdateUserRequest
                {
                    UserName = UserName,
                    Email = Email,
                    FullName = FullName
                };

                // Actualizar información básica del usuario
                var updatedUser = await _userApiClient.UpdateUserAsync(UserId, updateRequest);

                // Activar/desactivar según estado
                if (updatedUser.IsActive != IsActive)
                {
                    if (IsActive)
                    {
                        updatedUser = await _userApiClient.ActivateUserAsync(UserId);
                    }
                    else
                    {
                        updatedUser = await _userApiClient.DeactivateUserAsync(UserId);
                    }
                }

                // Cambiar contraseña si se ingresó una nueva
                if (!string.IsNullOrEmpty(NewPassword))
                {
                    await _userApiClient.ChangePasswordAsync(UserId, NewPassword);
                }

                // Actualizar roles
                // Primero necesitamos conocer qué roles tiene actualmente el usuario
                var currentUser = await _userApiClient.GetUserByIdAsync(UserId);
                var currentRoles = currentUser.Roles.ToList();

                // Roles a agregar (están en selectedRoles pero no en currentRoles)
                var rolesToAdd = SelectedRoles.Except(currentRoles).ToList();
                foreach (var roleId in rolesToAdd)
                {
                    await _userApiClient.AssignRoleToUserAsync(UserId, roleId);
                }

                // Roles a quitar (están en currentRoles pero no en selectedRoles)
                var rolesToRemove = currentRoles.Except(SelectedRoles).ToList();
                foreach (var roleId in rolesToRemove)
                {
                    await _userApiClient.RevokeRoleFromUserAsync(UserId, roleId);
                }

                // Recargar el usuario para obtener datos actualizados
                updatedUser = await _userApiClient.GetUserByIdAsync(UserId);

                SuccessMessage = "Usuario actualizado exitosamente";
                return updatedUser;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al actualizar usuario: {ex.Message}";
                return null;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Verifica si los campos del modelo son válidos
        /// </summary>
        public bool IsValid()
        {
            var context = new ValidationContext(this);
            var results = new List<ValidationResult>();

            // Validar solo NewPassword si está presente
            if (string.IsNullOrEmpty(NewPassword))
            {
                // Crear un nuevo contexto sin incluir las propiedades de contraseña
                var propertiesToValidate = new List<string> { nameof(UserName), nameof(Email), nameof(FullName) };
                var isValid = true;

                foreach (var propName in propertiesToValidate)
                {
                    var value = GetType().GetProperty(propName)?.GetValue(this);
                    var validationContext = new ValidationContext(this) { MemberName = propName };
                    isValid &= Validator.TryValidateProperty(value, validationContext, results);
                }

                return isValid;
            }
            else
            {
                return Validator.TryValidateObject(this, context, results, true);
            }
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