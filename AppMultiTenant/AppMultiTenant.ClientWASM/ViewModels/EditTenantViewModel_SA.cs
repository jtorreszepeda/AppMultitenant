using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using AppMultiTenant.ClientWASM.Services;

namespace AppMultiTenant.ClientWASM.ViewModels
{
    /// <summary>
    /// ViewModel para la vista de edición de inquilinos (Super Administrador)
    /// </summary>
    public class EditTenantViewModel_SA : INotifyPropertyChanged
    {
        private readonly ITenantApiClient _tenantApiClient;
        private bool _isLoading;
        private bool _isSaved;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private string _tenantId = string.Empty;
        private string _tenantName = string.Empty;
        private string _tenantIdentifier = string.Empty;
        private bool _isActive = true;
        private string _createdDate = string.Empty;
        private TenantStatistics _statistics = new TenantStatistics();

        /// <summary>
        /// Constructor del ViewModel de edición de inquilinos
        /// </summary>
        /// <param name="tenantApiClient">Cliente API para operaciones con inquilinos</param>
        public EditTenantViewModel_SA(ITenantApiClient tenantApiClient)
        {
            _tenantApiClient = tenantApiClient ?? throw new ArgumentNullException(nameof(tenantApiClient));
        }

        /// <summary>
        /// Indica si se está cargando datos o procesando cambios
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Indica si los cambios han sido guardados exitosamente
        /// </summary>
        public bool IsSaved
        {
            get => _isSaved;
            set => SetProperty(ref _isSaved, value);
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
        /// Mensaje de éxito tras guardar cambios
        /// </summary>
        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        /// <summary>
        /// ID del inquilino que se está editando
        /// </summary>
        public string TenantId
        {
            get => _tenantId;
            set => SetProperty(ref _tenantId, value);
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
        /// Indica si el inquilino está activo
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        /// <summary>
        /// Fecha de creación del inquilino (solo lectura)
        /// </summary>
        public string CreatedDate
        {
            get => _createdDate;
            set => SetProperty(ref _createdDate, value);
        }

        /// <summary>
        /// Estadísticas del inquilino
        /// </summary>
        public TenantStatistics Statistics
        {
            get => _statistics;
            set => SetProperty(ref _statistics, value);
        }

        /// <summary>
        /// Inicializa el ViewModel cargando los datos del inquilino
        /// </summary>
        /// <param name="tenantId">ID del inquilino a editar</param>
        public async Task InitializeAsync(string tenantId)
        {
            if (string.IsNullOrEmpty(tenantId))
            {
                ErrorMessage = "ID de inquilino no válido";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;
                IsSaved = false;
                TenantId = tenantId;

                // Cargar datos del inquilino
                var tenant = await _tenantApiClient.GetTenantByIdAsync(tenantId);
                TenantName = tenant.Name;
                TenantIdentifier = tenant.Identifier;
                IsActive = tenant.IsActive;
                CreatedDate = tenant.CreatedDate;

                // Cargar estadísticas
                try
                {
                    Statistics = await _tenantApiClient.GetTenantStatisticsAsync(tenantId);
                }
                catch
                {
                    // Si falla la carga de estadísticas, no interrumpir la carga principal
                    Statistics = new TenantStatistics();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar el inquilino: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Guarda los cambios en el inquilino
        /// </summary>
        public async Task SaveChangesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;
                IsSaved = false;

                var request = new UpdateTenantRequest
                {
                    Name = TenantName,
                    Identifier = TenantIdentifier,
                    IsActive = IsActive
                };

                var tenant = await _tenantApiClient.UpdateTenantAsync(TenantId, request);

                IsSaved = true;
                SuccessMessage = $"Inquilino '{tenant.Name}' actualizado exitosamente";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al actualizar el inquilino: {ex.Message}";
                IsSaved = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Activa un inquilino desactivado
        /// </summary>
        public async Task ActivateTenantAsync()
        {
            if (IsActive)
                return; // Ya está activo

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                var tenant = await _tenantApiClient.ActivateTenantAsync(TenantId);
                IsActive = tenant.IsActive;

                SuccessMessage = $"Inquilino '{tenant.Name}' activado exitosamente";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al activar el inquilino: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Desactiva un inquilino activo
        /// </summary>
        public async Task DeactivateTenantAsync()
        {
            if (!IsActive)
                return; // Ya está inactivo

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                var tenant = await _tenantApiClient.DeactivateTenantAsync(TenantId);
                IsActive = tenant.IsActive;

                SuccessMessage = $"Inquilino '{tenant.Name}' desactivado exitosamente";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al desactivar el inquilino: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Elimina el inquilino (operación peligrosa)
        /// </summary>
        /// <returns>True si la eliminación fue exitosa, False en caso contrario</returns>
        public async Task<bool> DeleteTenantAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                await _tenantApiClient.DeleteTenantAsync(TenantId);

                SuccessMessage = "Inquilino eliminado exitosamente";
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al eliminar el inquilino: {ex.Message}";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Refresca las estadísticas del inquilino
        /// </summary>
        public async Task RefreshStatisticsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                Statistics = await _tenantApiClient.GetTenantStatisticsAsync(TenantId);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar estadísticas: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
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