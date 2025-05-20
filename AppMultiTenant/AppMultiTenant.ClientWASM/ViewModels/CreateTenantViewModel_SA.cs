using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using AppMultiTenant.ClientWASM.Services;

namespace AppMultiTenant.ClientWASM.ViewModels
{
    /// <summary>
    /// ViewModel para la vista de creación de inquilinos (Super Administrador)
    /// </summary>
    public class CreateTenantViewModel_SA : INotifyPropertyChanged
    {
        private readonly ITenantApiClient _tenantApiClient;
        private bool _isLoading;
        private bool _isCreated;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private string _tenantName = string.Empty;
        private string _tenantIdentifier = string.Empty;
        private bool _isActive = true;
        private string _adminUserName = string.Empty;
        private string _adminFullName = string.Empty;
        private string _adminEmail = string.Empty;
        private string _adminPassword = string.Empty;

        /// <summary>
        /// Constructor del ViewModel de creación de inquilinos
        /// </summary>
        /// <param name="tenantApiClient">Cliente API para operaciones con inquilinos</param>
        public CreateTenantViewModel_SA(ITenantApiClient tenantApiClient)
        {
            _tenantApiClient = tenantApiClient ?? throw new ArgumentNullException(nameof(tenantApiClient));
        }

        /// <summary>
        /// Indica si se está procesando la creación
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Indica si el inquilino ha sido creado exitosamente
        /// </summary>
        public bool IsCreated
        {
            get => _isCreated;
            set => SetProperty(ref _isCreated, value);
        }

        /// <summary>
        /// Mensaje de error si ocurre algún problema
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>
        /// Mensaje de éxito tras la creación
        /// </summary>
        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        /// <summary>
        /// Nombre del inquilino
        /// </summary>
        [Required(ErrorMessage = "El nombre del inquilino es requerido")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        public string TenantName
        {
            get => _tenantName;
            set => SetProperty(ref _tenantName, value);
        }

        /// <summary>
        /// Identificador único del inquilino para acceso (ej. para subdominio)
        /// </summary>
        [Required(ErrorMessage = "El identificador es requerido")]
        [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "El identificador solo puede contener letras minúsculas, números y guiones")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El identificador debe tener entre 3 y 50 caracteres")]
        public string TenantIdentifier
        {
            get => _tenantIdentifier;
            set => SetProperty(ref _tenantIdentifier, value);
        }

        /// <summary>
        /// Indica si el inquilino estará activo inicialmente
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        /// <summary>
        /// Nombre de usuario del administrador inicial
        /// </summary>
        [Required(ErrorMessage = "El nombre de usuario del administrador es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        public string AdminUserName
        {
            get => _adminUserName;
            set => SetProperty(ref _adminUserName, value);
        }

        /// <summary>
        /// Nombre completo del administrador inicial
        /// </summary>
        [Required(ErrorMessage = "El nombre completo del administrador es requerido")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre completo debe tener entre 3 y 100 caracteres")]
        public string AdminFullName
        {
            get => _adminFullName;
            set => SetProperty(ref _adminFullName, value);
        }

        /// <summary>
        /// Correo electrónico del administrador inicial
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico del administrador es requerido")]
        [EmailAddress(ErrorMessage = "Debe ser un correo electrónico válido")]
        public string AdminEmail
        {
            get => _adminEmail;
            set => SetProperty(ref _adminEmail, value);
        }

        /// <summary>
        /// Contraseña del administrador inicial
        /// </summary>
        [Required(ErrorMessage = "La contraseña del administrador es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string AdminPassword
        {
            get => _adminPassword;
            set => SetProperty(ref _adminPassword, value);
        }

        /// <summary>
        /// Crea un nuevo inquilino con los datos proporcionados
        /// </summary>
        public async Task CreateTenantAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;
                IsCreated = false;

                var request = new CreateTenantRequest
                {
                    Name = TenantName,
                    Identifier = TenantIdentifier,
                    IsActive = IsActive,
                    AdminUser = new AdminUserInfo
                    {
                        UserName = AdminUserName,
                        Email = AdminEmail,
                        Password = AdminPassword,
                        FullName = AdminFullName
                    }
                };

                var tenant = await _tenantApiClient.CreateTenantAsync(request);

                IsCreated = true;
                SuccessMessage = $"Inquilino '{tenant.Name}' creado exitosamente con ID: {tenant.Id}";

                // Limpiar el formulario para una nueva creación
                ResetForm();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al crear el inquilino: {ex.Message}";
                IsCreated = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Reinicia el formulario a sus valores iniciales
        /// </summary>
        public void ResetForm()
        {
            TenantName = string.Empty;
            TenantIdentifier = string.Empty;
            IsActive = true;
            AdminUserName = string.Empty;
            AdminFullName = string.Empty;
            AdminEmail = string.Empty;
            AdminPassword = string.Empty;
            ErrorMessage = string.Empty;
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