namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Interfaz para el cliente de API de gestión de definiciones de secciones
    /// </summary>
    public interface ISectionApiClient : IApiClient
    {
        /// <summary>
        /// Obtiene todas las definiciones de secciones del inquilino actual
        /// </summary>
        /// <returns>Lista de definiciones de secciones</returns>
        Task<IEnumerable<SectionDefinitionDto>> GetAllSectionDefinitionsAsync();

        /// <summary>
        /// Obtiene una definición de sección por su ID
        /// </summary>
        /// <param name="sectionId">ID de la sección</param>
        /// <returns>Definición de sección o null si no existe</returns>
        Task<SectionDefinitionDto> GetSectionDefinitionByIdAsync(Guid sectionId);

        /// <summary>
        /// Crea una nueva definición de sección en el inquilino actual
        /// </summary>
        /// <param name="sectionDto">Datos de la sección a crear</param>
        /// <returns>Definición de sección creada o null en caso de error</returns>
        Task<SectionDefinitionDto> CreateSectionDefinitionAsync(CreateSectionDefinitionDto sectionDto);

        /// <summary>
        /// Actualiza los datos de una definición de sección existente
        /// </summary>
        /// <param name="sectionId">ID de la sección</param>
        /// <param name="sectionDto">Datos actualizados de la sección</param>
        /// <returns>Definición de sección actualizada o null en caso de error</returns>
        Task<SectionDefinitionDto> UpdateSectionDefinitionAsync(Guid sectionId, UpdateSectionDefinitionDto sectionDto);

        /// <summary>
        /// Elimina una definición de sección
        /// </summary>
        /// <param name="sectionId">ID de la sección a eliminar</param>
        /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
        Task<bool> DeleteSectionDefinitionAsync(Guid sectionId);

        /// <summary>
        /// Obtiene las secciones disponibles para el usuario actual con sus permisos
        /// </summary>
        /// <returns>Lista de secciones disponibles con permisos</returns>
        Task<IEnumerable<SectionInfo>> GetAvailableSectionsForUserAsync();
    }

    /// <summary>
    /// DTO para representar una definición de sección en la UI
    /// </summary>
    public class SectionDefinitionDto
    {
        /// <summary>
        /// ID de la definición de sección
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre de la sección
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción de la sección
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Icono para la sección
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Fecha de creación
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// ID del inquilino al que pertenece
        /// </summary>
        public Guid TenantId { get; set; }
    }

    /// <summary>
    /// DTO para crear una nueva definición de sección
    /// </summary>
    public class CreateSectionDefinitionDto
    {
        /// <summary>
        /// Nombre de la sección
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción de la sección
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Icono para la sección (opcional)
        /// </summary>
        public string Icon { get; set; }
    }

    /// <summary>
    /// DTO para actualizar una definición de sección existente
    /// </summary>
    public class UpdateSectionDefinitionDto
    {
        /// <summary>
        /// Nombre de la sección
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción de la sección
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Icono para la sección
        /// </summary>
        public string Icon { get; set; }
    }
} 