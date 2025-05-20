using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using AppMultiTenant.ClientWASM.Services;

namespace AppMultiTenant.ClientWASM.ViewModels
{
    /// <summary>
    /// ViewModel para la creación de roles
    /// </summary>
    public class CreateRoleViewModel : INotifyPropertyChanged
    {
        private readonly IRoleApiClient _roleApiClient;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private List<string> _selectedPermissions = new();
        private List<PermissionDto> _availablePermissions = new();

        public CreateRoleViewModel(IRoleApiClient roleApiClient)
        {
            _roleApiClient = roleApiClient ?? throw new ArgumentNullException(nameof(roleApiClient));
        }

        /// <summary>
        /// Nombre del rol
        /// </summary>
        [Required(ErrorMessage = "El nombre del rol es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre del rol debe tener entre 3 y 50 caracteres")]
        [Display(Name = "Nombre del rol")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del rol
        /// </summary>
        [StringLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres")]
        [Display(Name = "Descripción")]
        public string Description { get; set; } = string.Empty;

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
        /// Permisos disponibles para asignar
        /// </summary>
        public List<PermissionDto> AvailablePermissions
        {
            get => _availablePermissions;
            set => SetProperty(ref _availablePermissions, value);
        }

        /// <summary>
        /// IDs de permisos seleccionados
        /// </summary>
        public List<string> SelectedPermissions
        {
            get => _selectedPermissions;
            set => SetProperty(ref _selectedPermissions, value);
        }

        /// <summary>
        /// Carga los permisos disponibles desde la API
        /// </summary>
        public async Task LoadPermissionsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var permissions = await _roleApiClient.GetAllPermissionsAsync();
                AvailablePermissions = permissions.ToList();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar permisos: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Envía los datos para crear un nuevo rol
        /// </summary>
        public async Task<RoleDto> CreateRoleAsync()
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
                var createRequest = new CreateRoleRequest
                {
                    Name = Name,
                    Description = Description
                };

                // Crea el rol
                var createdRole = await _roleApiClient.CreateRoleAsync(createRequest);

                // Asigna permisos si hay seleccionados
                if (SelectedPermissions.Count > 0 && createdRole != null)
                {
                    await _roleApiClient.AssignPermissionsToRoleAsync(createdRole.Id, SelectedPermissions);

                    // Recarga el rol para obtener los permisos actualizados
                    createdRole = await _roleApiClient.GetRoleByIdAsync(createdRole.Id);
                }

                SuccessMessage = "Rol creado exitosamente";
                ResetForm();
                return createdRole;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al crear rol: {ex.Message}";
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
            Name = string.Empty;
            Description = string.Empty;
            SelectedPermissions = new List<string>();
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