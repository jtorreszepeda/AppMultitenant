using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using AppMultiTenant.ClientWASM.Services;

namespace AppMultiTenant.ClientWASM.ViewModels
{
    /// <summary>
    /// ViewModel para la edición de roles
    /// </summary>
    public class EditRoleViewModel : INotifyPropertyChanged
    {
        private readonly IRoleApiClient _roleApiClient;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private List<string> _selectedPermissions = new();
        private List<PermissionDto> _availablePermissions = new();
        private string _roleId = string.Empty;

        public EditRoleViewModel(IRoleApiClient roleApiClient)
        {
            _roleApiClient = roleApiClient ?? throw new ArgumentNullException(nameof(roleApiClient));
        }

        /// <summary>
        /// ID del rol que se está editando
        /// </summary>
        public string RoleId
        {
            get => _roleId;
            set => SetProperty(ref _roleId, value);
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
        /// Carga los datos del rol por su ID
        /// </summary>
        /// <param name="roleId">ID del rol a cargar</param>
        public async Task LoadRoleAsync(string roleId)
        {
            try
            {
                if (string.IsNullOrEmpty(roleId))
                {
                    ErrorMessage = "ID de rol no especificado";
                    return;
                }

                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;
                RoleId = roleId;

                // Cargar datos del rol
                var role = await _roleApiClient.GetRoleByIdAsync(roleId);

                // Actualizar propiedades del ViewModel
                Name = role.Name;
                Description = role.Description;

                // Cargar permisos del rol
                var assignedPermissions = await _roleApiClient.GetPermissionsByRoleIdAsync(roleId);
                SelectedPermissions = assignedPermissions.Select(p => p.Id).ToList();

                // Cargar todos los permisos disponibles
                await LoadPermissionsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar datos del rol: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Carga los permisos disponibles desde la API
        /// </summary>
        public async Task LoadPermissionsAsync()
        {
            try
            {
                var permissions = await _roleApiClient.GetAllPermissionsAsync();
                AvailablePermissions = permissions.ToList();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar permisos: {ex.Message}";
            }
        }

        /// <summary>
        /// Actualiza los datos del rol
        /// </summary>
        public async Task<RoleDto> UpdateRoleAsync()
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
                var updateRequest = new UpdateRoleRequest
                {
                    Name = Name,
                    Description = Description
                };

                // Actualizar información básica del rol
                var updatedRole = await _roleApiClient.UpdateRoleAsync(RoleId, updateRequest);

                // Actualizar permisos
                // Primero necesitamos conocer qué permisos tiene actualmente el rol
                var currentRole = await _roleApiClient.GetRoleByIdAsync(RoleId);
                var currentPermissionIds = currentRole.Permissions.Select(p => p.Id).ToList();

                // Determinar permisos a añadir (están en SelectedPermissions pero no en los actuales)
                var permissionsToAdd = SelectedPermissions
                    .Where(id => !currentPermissionIds.Contains(id))
                    .ToList();

                // Determinar permisos a eliminar (están en los actuales pero no en SelectedPermissions)
                var permissionsToRemove = currentPermissionIds
                    .Where(id => !SelectedPermissions.Contains(id))
                    .ToList();

                // Aplicar cambios
                if (permissionsToAdd.Any())
                {
                    await _roleApiClient.AssignPermissionsToRoleAsync(RoleId, permissionsToAdd);
                }

                if (permissionsToRemove.Any())
                {
                    await _roleApiClient.RevokePermissionsFromRoleAsync(RoleId, permissionsToRemove);
                }

                // Recargar el rol con permisos actualizados
                updatedRole = await _roleApiClient.GetRoleByIdAsync(RoleId);

                SuccessMessage = "Rol actualizado exitosamente";
                return updatedRole;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al actualizar rol: {ex.Message}";
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