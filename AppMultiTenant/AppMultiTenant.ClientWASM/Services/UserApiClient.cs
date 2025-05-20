using System.Net.Http.Json;
using System.Text.Json;

namespace AppMultiTenant.ClientWASM.Services
{
    /// <summary>
    /// Implementación del cliente para llamadas a la API de usuarios
    /// </summary>
    public class UserApiClient : IUserApiClient
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Constructor para el cliente de API de usuarios
        /// </summary>
        /// <param name="httpClient">Cliente HTTP para realizar peticiones</param>
        public UserApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Obtiene el usuario por su ID
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Datos del usuario</returns>
        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"api/tenant-users/{userId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions) ?? new UserDto();
        }

        /// <summary>
        /// Obtiene una lista paginada de usuarios del inquilino actual
        /// </summary>
        /// <param name="pageNumber">Número de página (empezando por 1)</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="includeInactive">Incluir usuarios inactivos</param>
        /// <returns>Lista paginada de usuarios</returns>
        public async Task<PagedResponse<UserDto>> GetUsersAsync(int pageNumber = 1, int pageSize = 10, bool includeInactive = false)
        {
            var url = $"api/tenant-users?pageNumber={pageNumber}&pageSize={pageSize}&includeInactive={includeInactive}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
            if (result != null)
            {
                return new PagedResponse<UserDto>
                {
                    Items = result.users.ToObject<IEnumerable<UserDto>>(),
                    TotalCount = result.totalCount,
                    PageNumber = result.pageNumber,
                    PageSize = result.pageSize
                };
            }

            return new PagedResponse<UserDto> { PageNumber = pageNumber, PageSize = pageSize };
        }

        /// <summary>
        /// Obtiene un usuario por su email
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <returns>Datos del usuario</returns>
        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            var response = await _httpClient.GetAsync($"api/tenant-users/by-email?email={Uri.EscapeDataString(email)}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions) ?? new UserDto();
        }

        /// <summary>
        /// Verifica si un email está disponible dentro del inquilino
        /// </summary>
        /// <param name="email">Email a verificar</param>
        /// <param name="excludeUserId">ID de usuario a excluir de la verificación (opcional)</param>
        /// <returns>True si está disponible, False si ya existe</returns>
        public async Task<bool> CheckEmailAvailabilityAsync(string email, string excludeUserId = null)
        {
            var url = $"api/tenant-users/check-email?email={Uri.EscapeDataString(email)}";
            if (!string.IsNullOrEmpty(excludeUserId))
            {
                url += $"&excludeUserId={excludeUserId}";
            }

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
            return result?.isAvailable ?? false;
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        /// <param name="request">Datos del usuario a crear</param>
        /// <returns>Datos del usuario creado</returns>
        public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/tenant-users", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions) ?? new UserDto();
        }

        /// <summary>
        /// Actualiza los datos de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario a actualizar</param>
        /// <param name="request">Datos a actualizar</param>
        /// <returns>Datos actualizados del usuario</returns>
        public async Task<UserDto> UpdateUserAsync(string userId, UpdateUserRequest request)
        {
            // Actualizar cada propiedad individualmente según los endpoints del controlador
            UserDto user = new UserDto();

            // Actualizar nombre completo
            if (!string.IsNullOrEmpty(request.FullName))
            {
                var fullNameResponse = await _httpClient.PutAsJsonAsync(
                    $"api/tenant-users/{userId}/fullname", 
                    new { FullName = request.FullName });
                fullNameResponse.EnsureSuccessStatusCode();
                user = await fullNameResponse.Content.ReadFromJsonAsync<UserDto>(_jsonOptions) ?? new UserDto();
            }

            // Actualizar email
            if (!string.IsNullOrEmpty(request.Email))
            {
                var emailResponse = await _httpClient.PutAsJsonAsync(
                    $"api/tenant-users/{userId}/email", 
                    new { Email = request.Email });
                emailResponse.EnsureSuccessStatusCode();
                user = await emailResponse.Content.ReadFromJsonAsync<UserDto>(_jsonOptions) ?? new UserDto();
            }

            // Actualizar nombre de usuario
            if (!string.IsNullOrEmpty(request.UserName))
            {
                var userNameResponse = await _httpClient.PutAsJsonAsync(
                    $"api/tenant-users/{userId}/username", 
                    new { UserName = request.UserName });
                userNameResponse.EnsureSuccessStatusCode();
                user = await userNameResponse.Content.ReadFromJsonAsync<UserDto>(_jsonOptions) ?? new UserDto();
            }

            return user;
        }

        /// <summary>
        /// Activa la cuenta de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Datos actualizados del usuario</returns>
        public async Task<UserDto> ActivateUserAsync(string userId)
        {
            var response = await _httpClient.PutAsync($"api/tenant-users/{userId}/activate", null);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions) ?? new UserDto();
        }

        /// <summary>
        /// Desactiva la cuenta de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Datos actualizados del usuario</returns>
        public async Task<UserDto> DeactivateUserAsync(string userId)
        {
            var response = await _httpClient.PutAsync($"api/tenant-users/{userId}/deactivate", null);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions) ?? new UserDto();
        }

        /// <summary>
        /// Elimina un usuario
        /// </summary>
        /// <param name="userId">ID del usuario a eliminar</param>
        /// <param name="currentUserId">ID del usuario que realiza la operación</param>
        /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
        public async Task<bool> DeleteUserAsync(string userId, string currentUserId)
        {
            var response = await _httpClient.DeleteAsync($"api/tenant-users/{userId}?currentUserId={currentUserId}");
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="newPassword">Nueva contraseña</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task ChangePasswordAsync(string userId, string newPassword)
        {
            var request = new { NewPassword = newPassword };
            var response = await _httpClient.PutAsJsonAsync($"api/tenant-users/{userId}/reset-password", request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Obtiene los roles asignados a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Lista de roles asignados al usuario</returns>
        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"api/tenant-users/{userId}/roles");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<string>>(_jsonOptions) ?? 
                   new List<string>();
        }

        /// <summary>
        /// Asigna un rol a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleId">ID del rol a asignar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task AssignRoleToUserAsync(string userId, string roleId)
        {
            var request = new { RoleIds = new[] { roleId } };
            var response = await _httpClient.PostAsJsonAsync($"api/tenant-users/{userId}/roles", request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Asigna múltiples roles a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleIds">IDs de los roles a asignar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task AssignRolesToUserAsync(string userId, IEnumerable<string> roleIds)
        {
            var request = new { RoleIds = roleIds };
            var response = await _httpClient.PostAsJsonAsync($"api/tenant-users/{userId}/roles", request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Revoca un rol de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleId">ID del rol a revocar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task RevokeRoleFromUserAsync(string userId, string roleId)
        {
            var request = new { RoleIds = new[] { roleId } };
            var content = JsonContent.Create(request, null, _jsonOptions);
            
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"api/tenant-users/{userId}/roles")
            {
                Content = content
            };
            
            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Revoca múltiples roles de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleIds">IDs de los roles a revocar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task RevokeRolesFromUserAsync(string userId, IEnumerable<string> roleIds)
        {
            var request = new { RoleIds = roleIds };
            var content = JsonContent.Create(request, null, _jsonOptions);
            
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"api/tenant-users/{userId}/roles")
            {
                Content = content
            };
            
            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();
        }
    }
}