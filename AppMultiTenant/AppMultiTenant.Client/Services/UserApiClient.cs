using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppMultiTenant.Client.Services
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
            
            return await response.Content.ReadFromJsonAsync<PagedResponse<UserDto>>(_jsonOptions) ?? 
                   new PagedResponse<UserDto> { PageNumber = pageNumber, PageSize = pageSize };
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
            var response = await _httpClient.PutAsJsonAsync($"api/tenant-users/{userId}", request);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions) ?? new UserDto();
        }

        /// <summary>
        /// Activa la cuenta de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Datos actualizados del usuario</returns>
        public async Task<UserDto> ActivateUserAsync(string userId)
        {
            var response = await _httpClient.PostAsync($"api/tenant-users/{userId}/activate", null);
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
            var response = await _httpClient.PostAsync($"api/tenant-users/{userId}/deactivate", null);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<UserDto>(_jsonOptions) ?? new UserDto();
        }

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="newPassword">Nueva contraseña</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task ChangePasswordAsync(string userId, string newPassword)
        {
            var request = new { Password = newPassword };
            var response = await _httpClient.PostAsJsonAsync($"api/tenant-users/{userId}/change-password", request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Asigna un rol a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleId">ID del rol a asignar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task AssignRoleToUserAsync(string userId, string roleId)
        {
            var response = await _httpClient.PostAsync($"api/tenant-users/{userId}/roles/{roleId}", null);
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
            var response = await _httpClient.DeleteAsync($"api/tenant-users/{userId}/roles/{roleId}");
            response.EnsureSuccessStatusCode();
        }
    }
} 