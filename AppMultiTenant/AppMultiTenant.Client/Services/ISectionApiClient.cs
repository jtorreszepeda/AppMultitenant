using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Modelo para representar una definición de sección en el cliente
    /// </summary>
    public class SectionDefinitionDto
    {
        /// <summary>
        /// Identificador único de la definición de sección
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de la sección
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción de la sección
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Identificador del inquilino al que pertenece la sección
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// Definiciones de campos de la sección
        /// </summary>
        public List<FieldDefinitionDto> Fields { get; set; } = new List<FieldDefinitionDto>();

        /// <summary>
        /// Fecha de creación de la sección
        /// </summary>
        public string CreatedDate { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de última modificación de la sección
        /// </summary>
        public string LastModifiedDate { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para representar una definición de campo en el cliente
    /// </summary>
    public class FieldDefinitionDto
    {
        /// <summary>
        /// Identificador único de la definición de campo
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del campo
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de datos del campo
        /// </summary>
        public string DataType { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el campo es requerido
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Orden de visualización del campo
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Configuración adicional específica del tipo de campo en formato JSON
        /// </summary>
        public string ConfigJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para crear una nueva definición de sección
    /// </summary>
    public class CreateSectionDefinitionRequest
    {
        /// <summary>
        /// Nombre de la sección
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción de la sección
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para actualizar una definición de sección existente
    /// </summary>
    public class UpdateSectionDefinitionRequest
    {
        /// <summary>
        /// Nombre de la sección
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción de la sección
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para crear una nueva definición de campo
    /// </summary>
    public class CreateFieldDefinitionRequest
    {
        /// <summary>
        /// Nombre del campo
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de datos del campo
        /// </summary>
        public string DataType { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el campo es requerido
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Orden de visualización del campo
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Configuración adicional específica del tipo de campo en formato JSON
        /// </summary>
        public string ConfigJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para actualizar una definición de campo existente
    /// </summary>
    public class UpdateFieldDefinitionRequest
    {
        /// <summary>
        /// Nombre del campo
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de datos del campo
        /// </summary>
        public string DataType { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el campo es requerido
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Orden de visualización del campo
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Configuración adicional específica del tipo de campo en formato JSON
        /// </summary>
        public string ConfigJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interfaz para el servicio cliente que encapsula las operaciones con definiciones de secciones
    /// </summary>
    public interface ISectionApiClient
    {
        /// <summary>
        /// Obtiene todas las definiciones de secciones del inquilino actual
        /// </summary>
        /// <returns>Lista de definiciones de secciones</returns>
        Task<IEnumerable<SectionDefinitionDto>> GetAllSectionDefinitionsAsync();

        /// <summary>
        /// Obtiene una definición de sección por su ID
        /// </summary>
        /// <param name="sectionId">ID de la definición de sección</param>
        /// <returns>Datos de la definición de sección</returns>
        Task<SectionDefinitionDto> GetSectionDefinitionByIdAsync(string sectionId);

        /// <summary>
        /// Crea una nueva definición de sección
        /// </summary>
        /// <param name="request">Datos de la sección a crear</param>
        /// <returns>Datos de la sección creada</returns>
        Task<SectionDefinitionDto> CreateSectionDefinitionAsync(CreateSectionDefinitionRequest request);

        /// <summary>
        /// Actualiza una definición de sección existente
        /// </summary>
        /// <param name="sectionId">ID de la sección a actualizar</param>
        /// <param name="request">Datos a actualizar</param>
        /// <returns>Datos actualizados de la sección</returns>
        Task<SectionDefinitionDto> UpdateSectionDefinitionAsync(string sectionId, UpdateSectionDefinitionRequest request);

        /// <summary>
        /// Elimina una definición de sección
        /// </summary>
        /// <param name="sectionId">ID de la sección a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task DeleteSectionDefinitionAsync(string sectionId);

        /// <summary>
        /// Obtiene todos los campos de una sección
        /// </summary>
        /// <param name="sectionId">ID de la sección</param>
        /// <returns>Lista de definiciones de campos</returns>
        Task<IEnumerable<FieldDefinitionDto>> GetFieldDefinitionsBySectionIdAsync(string sectionId);

        /// <summary>
        /// Crea un nuevo campo en una sección
        /// </summary>
        /// <param name="sectionId">ID de la sección</param>
        /// <param name="request">Datos del campo a crear</param>
        /// <returns>Datos del campo creado</returns>
        Task<FieldDefinitionDto> CreateFieldDefinitionAsync(string sectionId, CreateFieldDefinitionRequest request);

        /// <summary>
        /// Actualiza un campo existente
        /// </summary>
        /// <param name="fieldId">ID del campo a actualizar</param>
        /// <param name="request">Datos a actualizar</param>
        /// <returns>Datos actualizados del campo</returns>
        Task<FieldDefinitionDto> UpdateFieldDefinitionAsync(string fieldId, UpdateFieldDefinitionRequest request);

        /// <summary>
        /// Elimina un campo
        /// </summary>
        /// <param name="fieldId">ID del campo a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task DeleteFieldDefinitionAsync(string fieldId);
    }
} 