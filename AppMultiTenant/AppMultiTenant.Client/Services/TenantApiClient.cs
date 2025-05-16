namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Implementación del cliente de API para gestión de inquilinos
    /// </summary>
    public class TenantApiClient : ApiClientBase, ITenantApiClient
    {
        private const string AdminBaseEndpoint = "/api/system/tenants";
        private const string TenantInfoEndpoint = "/api/tenant/info";

        /// <summary>
        /// Constructor del cliente de API para gestión de inquilinos
        /// </summary>
        /// <param name="httpClient">Cliente HTTP configurado</param>
        /// <param name="logger">Servicio de logging</param>
        public TenantApiClient(HttpClient httpClient, ILogger<TenantApiClient> logger)
            : base(httpClient, logger)
        {
        }

        /// <summary>
        /// Obtiene todos los inquilinos del sistema
        /// </summary>
        /// <returns>Lista de inquilinos</returns>
        public async Task<IEnumerable<TenantDto>> GetAllTenantsAsync()
        {
            return await GetAsync<IEnumerable<TenantDto>>(AdminBaseEndpoint);
        }

        /// <summary>
        /// Obtiene un inquilino por su ID
        /// </summary>
        /// <param name="tenantId">ID del inquilino</param>
        /// <returns>Datos del inquilino o null si no existe</returns>
        public async Task<TenantDto> GetTenantByIdAsync(Guid tenantId)
        {
            return await GetAsyncSafe<TenantDto>($"{AdminBaseEndpoint}/{tenantId}");
        }

        /// <summary>
        /// Crea un nuevo inquilino en el sistema
        /// </summary>
        /// <param name="tenantDto">Datos del inquilino a crear</param>
        /// <returns>Datos del inquilino creado o null en caso de error</returns>
        public async Task<TenantDto> CreateTenantAsync(CreateTenantDto tenantDto)
        {
            try
            {
                return await PostAsync<CreateTenantDto, TenantDto>(AdminBaseEndpoint, tenantDto);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al crear inquilino con nombre {Name}", tenantDto.Name);
                throw;
            }
        }

        /// <summary>
        /// Actualiza los datos de un inquilino existente
        /// </summary>
        /// <param name="tenantId">ID del inquilino</param>
        /// <param name="tenantDto">Datos actualizados del inquilino</param>
        /// <returns>Datos del inquilino actualizado o null en caso de error</returns>
        public async Task<TenantDto> UpdateTenantAsync(Guid tenantId, UpdateTenantDto tenantDto)
        {
            try
            {
                return await PutAsync<UpdateTenantDto, TenantDto>($"{AdminBaseEndpoint}/{tenantId}", tenantDto);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al actualizar inquilino con ID {TenantId}", tenantId);
                throw;
            }
        }

        /// <summary>
        /// Elimina un inquilino
        /// </summary>
        /// <param name="tenantId">ID del inquilino a eliminar</param>
        /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
        public async Task<bool> DeleteTenantAsync(Guid tenantId)
        {
            return await DeleteAsync($"{AdminBaseEndpoint}/{tenantId}");
        }

        /// <summary>
        /// Obtiene información del inquilino actual (para usuarios normales)
        /// </summary>
        /// <returns>Información básica del inquilino actual</returns>
        public async Task<TenantInfoDto> GetCurrentTenantInfoAsync()
        {
            return await GetAsyncSafe<TenantInfoDto>(TenantInfoEndpoint);
        }
    }
} 