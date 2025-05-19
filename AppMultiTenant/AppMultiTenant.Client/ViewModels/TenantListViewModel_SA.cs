using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AppMultiTenant.Client.Services;

namespace AppMultiTenant.Client.ViewModels
{
    /// <summary>
    /// ViewModel para la vista de listado de inquilinos (Super Administrador)
    /// </summary>
    public class TenantListViewModel_SA : INotifyPropertyChanged
    {
        private readonly ITenantApiClient _tenantApiClient;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private List<TenantDto> _tenants = new();
        private bool _includeInactive;
        private string _searchTerm = string.Empty;

        /// <summary>
        /// Constructor del ViewModel de listado de inquilinos
        /// </summary>
        /// <param name="tenantApiClient">Cliente API para operaciones con inquilinos</param>
        public TenantListViewModel_SA(ITenantApiClient tenantApiClient)
        {
            _tenantApiClient = tenantApiClient ?? throw new ArgumentNullException(nameof(tenantApiClient));
        }

        /// <summary>
        /// Indica si se está cargando datos
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
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
        /// Lista de inquilinos
        /// </summary>
        public List<TenantDto> Tenants
        {
            get => _tenants;
            set => SetProperty(ref _tenants, value);
        }

        /// <summary>
        /// Indica si se deben incluir inquilinos inactivos
        /// </summary>
        public bool IncludeInactive
        {
            get => _includeInactive;
            set => SetProperty(ref _includeInactive, value);
        }
        
        /// <summary>
        /// Término de búsqueda para filtrar inquilinos
        /// </summary>
        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        /// <summary>
        /// Carga la lista de inquilinos desde la API
        /// </summary>
        public async Task LoadTenantsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var allTenants = await _tenantApiClient.GetAllTenantsAsync();
                
                // Filtrar localmente según los criterios
                var filteredTenants = new List<TenantDto>();
                
                foreach (var tenant in allTenants)
                {
                    // Filtrar por estado activo/inactivo
                    if (!IncludeInactive && !tenant.IsActive)
                        continue;
                    
                    // Filtrar por término de búsqueda
                    if (!string.IsNullOrWhiteSpace(SearchTerm))
                    {
                        var searchTermLower = SearchTerm.ToLowerInvariant();
                        if (!tenant.Name.ToLowerInvariant().Contains(searchTermLower) && 
                            !tenant.Identifier.ToLowerInvariant().Contains(searchTermLower))
                            continue;
                    }
                    
                    filteredTenants.Add(tenant);
                }
                
                Tenants = filteredTenants;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar inquilinos: {ex.Message}";
                Tenants = new List<TenantDto>();
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Activa un inquilino desactivado
        /// </summary>
        /// <param name="tenantId">ID del inquilino a activar</param>
        public async Task ActivateTenantAsync(string tenantId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                await _tenantApiClient.ActivateTenantAsync(tenantId);
                await LoadTenantsAsync(); // Recargar la lista tras la activación
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al activar inquilino: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Desactiva un inquilino activo
        /// </summary>
        /// <param name="tenantId">ID del inquilino a desactivar</param>
        public async Task DeactivateTenantAsync(string tenantId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                await _tenantApiClient.DeactivateTenantAsync(tenantId);
                await LoadTenantsAsync(); // Recargar la lista tras la desactivación
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al desactivar inquilino: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Elimina un inquilino (operación peligrosa)
        /// </summary>
        /// <param name="tenantId">ID del inquilino a eliminar</param>
        public async Task DeleteTenantAsync(string tenantId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                await _tenantApiClient.DeleteTenantAsync(tenantId);
                await LoadTenantsAsync(); // Recargar la lista tras la eliminación
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al eliminar inquilino: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Cambia la opción de incluir inquilinos inactivos y recarga la lista
        /// </summary>
        public async Task ToggleIncludeInactiveAsync()
        {
            IncludeInactive = !IncludeInactive;
            await LoadTenantsAsync();
        }

        /// <summary>
        /// Aplica el filtro de búsqueda y recarga la lista
        /// </summary>
        public async Task ApplySearchFilterAsync()
        {
            await LoadTenantsAsync();
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