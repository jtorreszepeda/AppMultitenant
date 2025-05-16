namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Implementación del cliente de API para gestión de roles
    /// </summary>
    public class RoleApiClient : ApiClientBase, IRoleApiClient
    {
        private const string BaseEndpoint = "/api/tenant/roles";
        private const string PermissionsEndpoint = "/api/tenant/permissions";

        /// <summary>
        /// Constructor del cliente de API para gestión de roles
        /// </summary>
        /// <param name="httpClient">Cliente HTTP configurado</param>
        /// <param name="logger">Servicio de logging</param>
        public RoleApiClient(HttpClient httpClient, ILogger<RoleApiClient> logger)
            : base(httpClient, logger)
        {
        }

        /// <summary>
        /// Obtiene todos los roles del inquilino actual
        /// </summary>
        /// <returns>Lista de roles</returns>
        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            return await GetAsync<IEnumerable<RoleDto>>(BaseEndpoint);
        }

        /// <summary>
        /// Obtiene un rol por su ID
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <returns>Datos del rol o null si no existe</returns>
        public async Task<RoleDto> GetRoleByIdAsync(Guid roleId)
        {
            return await GetAsyncSafe<RoleDto>($"{BaseEndpoint}/{roleId}");
        }

        /// <summary>
        /// Crea un nuevo rol en el inquilino actual
        /// </summary>
        /// <param name="roleDto">Datos del rol a crear</param>
        /// <returns>Datos del rol creado o null en caso de error</returns>
        public async Task<RoleDto> CreateRoleAsync(CreateRoleDto roleDto)
        {
            try
            {
                return await PostAsync<CreateRoleDto, RoleDto>(BaseEndpoint, roleDto);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al crear rol con nombre {Name}", roleDto.Name);
                throw;
            }
        }

        /// <summary>
        /// Actualiza los datos de un rol existente
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="roleDto">Datos actualizados del rol</param>
        /// <returns>Datos del rol actualizado o null en caso de error</returns>
        public async Task<RoleDto> UpdateRoleAsync(Guid roleId, UpdateRoleDto roleDto)
        {
            try
            {
                return await PutAsync<UpdateRoleDto, RoleDto>($"{BaseEndpoint}/{roleId}", roleDto);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al actualizar rol con ID {RoleId}", roleId);
                throw;
            }
        }

        /// <summary>
        /// Elimina un rol
        /// </summary>
        /// <param name="roleId">ID del rol a eliminar</param>
        /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
        public async Task<bool> DeleteRoleAsync(Guid roleId)
        {
            return await DeleteAsync($"{BaseEndpoint}/{roleId}");
        }

        /// <summary>
        /// Obtiene todos los permisos disponibles en el sistema
        /// </summary>
        /// <returns>Lista de permisos</returns>
        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
        {
            return await GetAsync<IEnumerable<PermissionDto>>(PermissionsEndpoint);
        }

        /// <summary>
        /// Obtiene los permisos asignados a un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <returns>Lista de permisos asignados al rol</returns>
        public async Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(Guid roleId)
        {
            return await GetAsync<IEnumerable<PermissionDto>>($"{BaseEndpoint}/{roleId}/permissions");
        }

        /// <summary>
        /// Asigna permisos a un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionIds">IDs de los permisos a asignar</param>
        /// <returns>True si se asignaron correctamente, False en caso contrario</returns>
        public async Task<bool> AssignPermissionsToRoleAsync(Guid roleId, IEnumerable<string> permissionIds)
        {
            try
            {
                var response = await PostAsync($"{BaseEndpoint}/{roleId}/permissions", permissionIds);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al asignar permisos al rol con ID {RoleId}", roleId);
                throw;
            }
        }
    }
} 