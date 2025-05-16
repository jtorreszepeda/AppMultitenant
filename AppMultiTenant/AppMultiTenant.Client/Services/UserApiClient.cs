namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Implementación del cliente de API para gestión de usuarios
    /// </summary>
    public class UserApiClient : ApiClientBase, IUserApiClient
    {
        private const string BaseEndpoint = "/api/tenant/users";

        /// <summary>
        /// Constructor del cliente de API para gestión de usuarios
        /// </summary>
        /// <param name="httpClient">Cliente HTTP configurado</param>
        /// <param name="logger">Servicio de logging</param>
        public UserApiClient(HttpClient httpClient, ILogger<UserApiClient> logger)
            : base(httpClient, logger)
        {
        }

        /// <summary>
        /// Obtiene todos los usuarios del inquilino actual
        /// </summary>
        /// <returns>Lista de usuarios</returns>
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await GetAsync<IEnumerable<UserDto>>(BaseEndpoint);
        }

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Datos del usuario o null si no existe</returns>
        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            return await GetAsyncSafe<UserDto>($"{BaseEndpoint}/{userId}");
        }

        /// <summary>
        /// Crea un nuevo usuario en el inquilino actual
        /// </summary>
        /// <param name="userDto">Datos del usuario a crear</param>
        /// <returns>Datos del usuario creado o null en caso de error</returns>
        public async Task<UserDto> CreateUserAsync(CreateUserDto userDto)
        {
            try
            {
                return await PostAsync<CreateUserDto, UserDto>(BaseEndpoint, userDto);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al crear usuario con email {Email}", userDto.Email);
                throw;
            }
        }

        /// <summary>
        /// Actualiza los datos de un usuario existente
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="userDto">Datos actualizados del usuario</param>
        /// <returns>Datos del usuario actualizado o null en caso de error</returns>
        public async Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserDto userDto)
        {
            try
            {
                return await PutAsync<UpdateUserDto, UserDto>($"{BaseEndpoint}/{userId}", userDto);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al actualizar usuario con ID {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Elimina un usuario
        /// </summary>
        /// <param name="userId">ID del usuario a eliminar</param>
        /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            return await DeleteAsync($"{BaseEndpoint}/{userId}");
        }

        /// <summary>
        /// Asigna roles a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleIds">IDs de los roles a asignar</param>
        /// <returns>True si se asignaron correctamente, False en caso contrario</returns>
        public async Task<bool> AssignRolesToUserAsync(Guid userId, IEnumerable<Guid> roleIds)
        {
            try
            {
                var response = await PostAsync($"{BaseEndpoint}/{userId}/roles", roleIds);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al asignar roles al usuario con ID {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene los roles asignados a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Lista de roles asignados al usuario</returns>
        public async Task<IEnumerable<RoleDto>> GetUserRolesAsync(Guid userId)
        {
            return await GetAsync<IEnumerable<RoleDto>>($"{BaseEndpoint}/{userId}/roles");
        }
    }
} 