using System.Text.Json;

namespace AppMultiTenant.Application.Interfaces.Services
{
    /// <summary>
    /// Servicio para manejar la serialización y deserialización segura de entidades de dominio directamente
    /// desde/hacia los controladores API.
    /// </summary>
    public interface IEntitySerializationService
    {
        /// <summary>
        /// Serializa una entidad de dominio para devolverla como respuesta HTTP.
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad de dominio</typeparam>
        /// <param name="entity">Entidad a serializar</param>
        /// <param name="options">Opciones de serialización opcionales</param>
        /// <returns>La entidad serializada</returns>
        string SerializeEntity<T>(T entity, JsonSerializerOptions? options = null);

        /// <summary>
        /// Deserializa un JSON en una entidad de dominio recibida en una solicitud HTTP.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad de dominio esperado</typeparam>
        /// <param name="json">JSON a deserializar</param>
        /// <param name="options">Opciones de deserialización opcionales</param>
        /// <returns>La entidad deserializada</returns>
        T? DeserializeEntity<T>(string json, JsonSerializerOptions? options = null);

        /// <summary>
        /// Configura opciones seguras de serialización con configuraciones predeterminadas.
        /// </summary>
        /// <returns>Opciones de serialización configuradas</returns>
        JsonSerializerOptions GetDefaultOptions();
    }
} 