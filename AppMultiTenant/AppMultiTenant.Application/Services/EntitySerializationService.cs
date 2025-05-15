using System.Text.Json;
using System.Text.Json.Serialization;
using AppMultiTenant.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AppMultiTenant.Application.Services
{
    /// <summary>
    /// Implementación del servicio para manejar la serialización y deserialización segura de entidades de dominio.
    /// </summary>
    public class EntitySerializationService : IEntitySerializationService
    {
        private readonly JsonSerializerOptions _defaultOptions;
        private readonly ILogger<EntitySerializationService> _logger;

        /// <summary>
        /// Constructor del servicio de serialización.
        /// </summary>
        /// <param name="logger">Servicio de logging</param>
        public EntitySerializationService(ILogger<EntitySerializationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _defaultOptions = CreateDefaultOptions();
        }

        /// <inheritdoc />
        public string SerializeEntity<T>(T entity, JsonSerializerOptions? options = null)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            try
            {
                return JsonSerializer.Serialize(entity, options ?? _defaultOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al serializar entidad de tipo {EntityType}", typeof(T).Name);
                throw new InvalidOperationException($"Error al serializar entidad de tipo {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public T? DeserializeEntity<T>(string json, JsonSerializerOptions? options = null)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentException("El JSON a deserializar no puede estar vacío", nameof(json));
            }

            try
            {
                return JsonSerializer.Deserialize<T>(json, options ?? _defaultOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al deserializar JSON a entidad de tipo {EntityType}: {Json}", typeof(T).Name, json);
                throw new InvalidOperationException($"Error al deserializar JSON a entidad de tipo {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public JsonSerializerOptions GetDefaultOptions()
        {
            // Devolvemos una copia de las opciones predeterminadas
            return new JsonSerializerOptions(_defaultOptions);
        }

        /// <summary>
        /// Crea las opciones predeterminadas de serialización con configuraciones seguras.
        /// </summary>
        /// <returns>Opciones de serialización configuradas</returns>
        private JsonSerializerOptions CreateDefaultOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                PropertyNameCaseInsensitive = true,
                WriteIndented = false,
                // Evitamos desactivar la validación para evitar vulnerabilidades de seguridad
                AllowTrailingCommas = false,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            return options;
        }
    }
}