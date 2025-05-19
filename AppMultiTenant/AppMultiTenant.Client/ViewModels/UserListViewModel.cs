using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AppMultiTenant.Client.Services;

namespace AppMultiTenant.Client.ViewModels
{
    /// <summary>
    /// ViewModel para la vista de listado de usuarios
    /// </summary>
    public class UserListViewModel : INotifyPropertyChanged
    {
        private readonly IUserApiClient _userApiClient;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private List<UserDto> _users = new();
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalCount;
        private int _totalPages;
        private bool _includeInactive;
        private string _searchTerm = string.Empty;

        /// <summary>
        /// Constructor del ViewModel de listado de usuarios
        /// </summary>
        /// <param name="userApiClient">Cliente API para operaciones con usuarios</param>
        public UserListViewModel(IUserApiClient userApiClient)
        {
            _userApiClient = userApiClient ?? throw new ArgumentNullException(nameof(userApiClient));
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
        /// Lista de usuarios actuales
        /// </summary>
        public List<UserDto> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        /// <summary>
        /// Página actual
        /// </summary>
        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        /// <summary>
        /// Tamaño de página
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => SetProperty(ref _pageSize, value);
        }

        /// <summary>
        /// Cantidad total de registros
        /// </summary>
        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        /// <summary>
        /// Número total de páginas
        /// </summary>
        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        /// <summary>
        /// Indica si se deben incluir usuarios inactivos
        /// </summary>
        public bool IncludeInactive
        {
            get => _includeInactive;
            set => SetProperty(ref _includeInactive, value);
        }
        
        /// <summary>
        /// Término de búsqueda para filtrar usuarios
        /// </summary>
        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        /// <summary>
        /// Carga la lista de usuarios desde la API
        /// </summary>
        public async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var response = await _userApiClient.GetUsersAsync(CurrentPage, PageSize, IncludeInactive);
                
                Users = new List<UserDto>(response.Items);
                TotalCount = response.TotalCount;
                TotalPages = response.TotalPages;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar usuarios: {ex.Message}";
                Users = new List<UserDto>();
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Navega a la página especificada
        /// </summary>
        /// <param name="page">Número de página</param>
        public async Task GoToPageAsync(int page)
        {
            if (page < 1 || page > TotalPages || page == CurrentPage)
                return;

            CurrentPage = page;
            await LoadUsersAsync();
        }

        /// <summary>
        /// Activa un usuario desactivado
        /// </summary>
        /// <param name="userId">ID del usuario a activar</param>
        public async Task ActivateUserAsync(string userId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                await _userApiClient.ActivateUserAsync(userId);
                await LoadUsersAsync(); // Recargar la lista tras la activación
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al activar usuario: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Desactiva un usuario activo
        /// </summary>
        /// <param name="userId">ID del usuario a desactivar</param>
        public async Task DeactivateUserAsync(string userId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                await _userApiClient.DeactivateUserAsync(userId);
                await LoadUsersAsync(); // Recargar la lista tras la desactivación
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al desactivar usuario: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Cambia la opción de incluir usuarios inactivos y recarga la lista
        /// </summary>
        public async Task ToggleIncludeInactiveAsync()
        {
            IncludeInactive = !IncludeInactive;
            CurrentPage = 1; // Volver a la primera página al cambiar el filtro
            await LoadUsersAsync();
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