using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AppMultiTenant.Client.Services;

namespace AppMultiTenant.Client.ViewModels
{
    /// <summary>
    /// ViewModel para la vista de listado de roles
    /// </summary>
    public class RoleListViewModel : INotifyPropertyChanged
    {
        private readonly IRoleApiClient _roleApiClient;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private List<RoleDto> _roles = new();
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalCount;
        private int _totalPages;
        private string _searchTerm = string.Empty;

        /// <summary>
        /// Constructor del ViewModel de listado de roles
        /// </summary>
        /// <param name="roleApiClient">Cliente API para operaciones con roles</param>
        public RoleListViewModel(IRoleApiClient roleApiClient)
        {
            _roleApiClient = roleApiClient ?? throw new ArgumentNullException(nameof(roleApiClient));
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
        /// Lista de roles actuales
        /// </summary>
        public List<RoleDto> Roles
        {
            get => _roles;
            set => SetProperty(ref _roles, value);
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
        /// Término de búsqueda para filtrar roles
        /// </summary>
        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        /// <summary>
        /// Carga la lista de roles desde la API
        /// </summary>
        public async Task LoadRolesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var roles = await _roleApiClient.GetAllRolesAsync();
                
                // Aplicar filtro si hay término de búsqueda
                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    var filteredRoles = new List<RoleDto>();
                    foreach (var role in roles)
                    {
                        if (role.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                            role.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                        {
                            filteredRoles.Add(role);
                        }
                    }
                    Roles = filteredRoles;
                }
                else
                {
                    Roles = new List<RoleDto>(roles);
                }
                
                // Si en el futuro la API soporta paginación, esto deberá actualizarse
                TotalCount = Roles.Count;
                TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
                
                // Implementación manual de paginación del lado del cliente
                if (Roles.Count > 0)
                {
                    var skip = (CurrentPage - 1) * PageSize;
                    if (skip < Roles.Count)
                    {
                        var take = Math.Min(PageSize, Roles.Count - skip);
                        Roles = Roles.GetRange(skip, take);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar roles: {ex.Message}";
                Roles = new List<RoleDto>();
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
            await LoadRolesAsync();
        }

        /// <summary>
        /// Busca roles por término
        /// </summary>
        public async Task SearchAsync()
        {
            CurrentPage = 1; // Volver a la primera página al cambiar el filtro
            await LoadRolesAsync();
        }

        /// <summary>
        /// Elimina un rol
        /// </summary>
        /// <param name="roleId">ID del rol a eliminar</param>
        public async Task DeleteRoleAsync(string roleId)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                await _roleApiClient.DeleteRoleAsync(roleId);
                await LoadRolesAsync(); // Recargar la lista tras la eliminación
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al eliminar rol: {ex.Message}";
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