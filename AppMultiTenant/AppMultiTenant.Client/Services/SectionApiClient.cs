namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Implementación del cliente de API para gestión de definiciones de secciones
    /// </summary>
    public class SectionApiClient : ApiClientBase, ISectionApiClient
    {
        private const string BaseEndpoint = "/api/tenant/sections";
        private const string AvailableSectionsEndpoint = "/api/tenant/sections/available";

        /// <summary>
        /// Constructor del cliente de API para gestión de secciones
        /// </summary>
        /// <param name="httpClient">Cliente HTTP configurado</param>
        /// <param name="logger">Servicio de logging</param>
        public SectionApiClient(HttpClient httpClient, ILogger<SectionApiClient> logger)
            : base(httpClient, logger)
        {
        }

        /// <summary>
        /// Obtiene todas las definiciones de secciones del inquilino actual
        /// </summary>
        /// <returns>Lista de definiciones de secciones</returns>
        public async Task<IEnumerable<SectionDefinitionDto>> GetAllSectionDefinitionsAsync()
        {
            return await GetAsync<IEnumerable<SectionDefinitionDto>>(BaseEndpoint);
        }

        /// <summary>
        /// Obtiene una definición de sección por su ID
        /// </summary>
        /// <param name="sectionId">ID de la sección</param>
        /// <returns>Definición de sección o null si no existe</returns>
        public async Task<SectionDefinitionDto> GetSectionDefinitionByIdAsync(Guid sectionId)
        {
            return await GetAsyncSafe<SectionDefinitionDto>($"{BaseEndpoint}/{sectionId}");
        }

        /// <summary>
        /// Crea una nueva definición de sección en el inquilino actual
        /// </summary>
        /// <param name="sectionDto">Datos de la sección a crear</param>
        /// <returns>Definición de sección creada o null en caso de error</returns>
        public async Task<SectionDefinitionDto> CreateSectionDefinitionAsync(CreateSectionDefinitionDto sectionDto)
        {
            try
            {
                return await PostAsync<CreateSectionDefinitionDto, SectionDefinitionDto>(BaseEndpoint, sectionDto);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al crear sección con nombre {Name}", sectionDto.Name);
                throw;
            }
        }

        /// <summary>
        /// Actualiza los datos de una definición de sección existente
        /// </summary>
        /// <param name="sectionId">ID de la sección</param>
        /// <param name="sectionDto">Datos actualizados de la sección</param>
        /// <returns>Definición de sección actualizada o null en caso de error</returns>
        public async Task<SectionDefinitionDto> UpdateSectionDefinitionAsync(Guid sectionId, UpdateSectionDefinitionDto sectionDto)
        {
            try
            {
                return await PutAsync<UpdateSectionDefinitionDto, SectionDefinitionDto>($"{BaseEndpoint}/{sectionId}", sectionDto);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al actualizar sección con ID {SectionId}", sectionId);
                throw;
            }
        }

        /// <summary>
        /// Elimina una definición de sección
        /// </summary>
        /// <param name="sectionId">ID de la sección a eliminar</param>
        /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
        public async Task<bool> DeleteSectionDefinitionAsync(Guid sectionId)
        {
            return await DeleteAsync($"{BaseEndpoint}/{sectionId}");
        }

        /// <summary>
        /// Obtiene las secciones disponibles para el usuario actual con sus permisos
        /// </summary>
        /// <returns>Lista de secciones disponibles con permisos</returns>
        public async Task<IEnumerable<SectionInfo>> GetAvailableSectionsForUserAsync()
        {
            try
            {
                return await GetAsync<IEnumerable<SectionInfo>>(AvailableSectionsEndpoint);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al obtener las secciones disponibles para el usuario");
                return Enumerable.Empty<SectionInfo>();
            }
        }
    }
} 