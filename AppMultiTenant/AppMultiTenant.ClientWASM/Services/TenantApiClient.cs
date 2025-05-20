using System.Text.Json;

namespace AppMultiTenant.ClientWASM.Services
{
    /// <summary>
    /// Implementación del cliente para llamadas a la API de inquilinos
    /// </summary>
    public class TenantApiClient : ITenantApiClient
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Constructor para el cliente de API de inquilinos
        /// </summary>
        /// <param name="httpClient">Cliente HTTP para realizar peticiones</param>
        public TenantApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Obtiene todos los inquilinos
        /// </summary>
        /// <returns>Lista de inquilinos</returns>
        public async Task<IEnumerable<TenantDto>> GetAllTenantsAsync()
        {
            var response = await _httpClient.GetAsync("api/system-admin/tenants");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<TenantDto>>(_jsonOptions) ??
                   new List<TenantDto>();
        }

        /// <summary>
        /// Obtiene un inquilino por su ID
        /// </summary>
        /// <param name="tenantId">ID del inquilino</param>
        /// <returns>Datos del inquilino</returns>
        public async Task<TenantDto> GetTenantByIdAsync(string tenantId)
        {
            var response = await _httpClient.GetAsync($"api/system-admin/tenants/{tenantId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TenantDto>(_jsonOptions) ?? new TenantDto();
        }

        /// <summary>
        /// Crea un nuevo inquilino
        /// </summary>
        /// <param name="request">Datos del inquilino a crear</param>
        /// <returns>Datos del inquilino creado</returns>
        public async Task<TenantDto> CreateTenantAsync(CreateTenantRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/system-admin/tenants", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TenantDto>(_jsonOptions) ?? new TenantDto();
        }

        /// <summary>
        /// Actualiza un inquilino existente
        /// </summary>
        /// <param name="tenantId">ID del inquilino a actualizar</param>
        /// <param name="request">Datos a actualizar</param>
        /// <returns>Datos actualizados del inquilino</returns>
        public async Task<TenantDto> UpdateTenantAsync(string tenantId, UpdateTenantRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/system-admin/tenants/{tenantId}", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TenantDto>(_jsonOptions) ?? new TenantDto();
        }

        /// <summary>
        /// Activa un inquilino
        /// </summary>
        /// <param name="tenantId">ID del inquilino</param>
        /// <returns>Datos actualizados del inquilino</returns>
        public async Task<TenantDto> ActivateTenantAsync(string tenantId)
        {
            var response = await _httpClient.PostAsync($"api/system-admin/tenants/{tenantId}/activate", null);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TenantDto>(_jsonOptions) ?? new TenantDto();
        }

        /// <summary>
        /// Desactiva un inquilino
        /// </summary>
        /// <param name="tenantId">ID del inquilino</param>
        /// <returns>Datos actualizados del inquilino</returns>
        public async Task<TenantDto> DeactivateTenantAsync(string tenantId)
        {
            var response = await _httpClient.PostAsync($"api/system-admin/tenants/{tenantId}/deactivate", null);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TenantDto>(_jsonOptions) ?? new TenantDto();
        }

        /// <summary>
        /// Elimina un inquilino (operación peligrosa)
        /// </summary>
        /// <param name="tenantId">ID del inquilino a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task DeleteTenantAsync(string tenantId)
        {
            var response = await _httpClient.DeleteAsync($"api/system-admin/tenants/{tenantId}");
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Obtiene estadísticas básicas de un inquilino
        /// </summary>
        /// <param name="tenantId">ID del inquilino</param>
        /// <returns>Estadísticas del inquilino</returns>
        public async Task<TenantStatistics> GetTenantStatisticsAsync(string tenantId)
        {
            var response = await _httpClient.GetAsync($"api/system-admin/tenants/{tenantId}/statistics");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TenantStatistics>(_jsonOptions) ??
                   new TenantStatistics();
        }
    }
}