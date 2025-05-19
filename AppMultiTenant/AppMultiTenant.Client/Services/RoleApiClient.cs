using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Implementación del cliente para llamadas a la API de roles y permisos
    /// </summary>
    public class RoleApiClient : IRoleApiClient
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Constructor para el cliente de API de roles
        /// </summary>
        /// <param name="httpClient">Cliente HTTP para realizar peticiones</param>
        public RoleApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Obtiene todos los roles del inquilino actual
        /// </summary>
        /// <returns>Lista de roles</returns>
        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var response = await _httpClient.GetAsync("api/tenant-roles");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<IEnumerable<RoleDto>>(_jsonOptions) ?? 
                   new List<RoleDto>();
        }

        /// <summary>
        /// Obtiene un rol por su ID
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <returns>Datos del rol</returns>
        public async Task<RoleDto> GetRoleByIdAsync(string roleId)
        {
            var response = await _httpClient.GetAsync($"api/tenant-roles/{roleId}");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<RoleDto>(_jsonOptions) ?? new RoleDto();
        }

        /// <summary>
        /// Crea un nuevo rol
        /// </summary>
        /// <param name="request">Datos del rol a crear</param>
        /// <returns>Datos del rol creado</returns>
        public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/tenant-roles", request);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<RoleDto>(_jsonOptions) ?? new RoleDto();
        }

        /// <summary>
        /// Actualiza un rol existente
        /// </summary>
        /// <param name="roleId">ID del rol a actualizar</param>
        /// <param name="request">Datos a actualizar</param>
        /// <returns>Datos actualizados del rol</returns>
        public async Task<RoleDto> UpdateRoleAsync(string roleId, UpdateRoleRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/tenant-roles/{roleId}", request);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<RoleDto>(_jsonOptions) ?? new RoleDto();
        }

        /// <summary>
        /// Elimina un rol
        /// </summary>
        /// <param name="roleId">ID del rol a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task DeleteRoleAsync(string roleId)
        {
            var response = await _httpClient.DeleteAsync($"api/tenant-roles/{roleId}");
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Obtiene todos los permisos disponibles en el sistema
        /// </summary>
        /// <returns>Lista de permisos</returns>
        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
        {
            var response = await _httpClient.GetAsync("api/tenant-roles/permissions");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<IEnumerable<PermissionDto>>(_jsonOptions) ?? 
                   new List<PermissionDto>();
        }

        /// <summary>
        /// Obtiene los permisos asignados a un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <returns>Lista de permisos asignados al rol</returns>
        public async Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(string roleId)
        {
            var response = await _httpClient.GetAsync($"api/tenant-roles/{roleId}/permissions");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<IEnumerable<PermissionDto>>(_jsonOptions) ?? 
                   new List<PermissionDto>();
        }

        /// <summary>
        /// Asigna permisos a un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionIds">IDs de los permisos a asignar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task AssignPermissionsToRoleAsync(string roleId, IEnumerable<string> permissionIds)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/tenant-roles/{roleId}/permissions", 
                new { PermissionIds = permissionIds });
            
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Revoca permisos de un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionIds">IDs de los permisos a revocar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task RevokePermissionsFromRoleAsync(string roleId, IEnumerable<string> permissionIds)
        {
            var requestUri = $"api/tenant-roles/{roleId}/permissions/revoke";
            var content = JsonContent.Create(new { PermissionIds = permissionIds });
            
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUri)
            {
                Content = content
            };
            
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
} 