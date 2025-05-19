using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Implementación del cliente para llamadas a la API de secciones
    /// </summary>
    public class SectionApiClient : ISectionApiClient
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Constructor para el cliente de API de secciones
        /// </summary>
        /// <param name="httpClient">Cliente HTTP para realizar peticiones</param>
        public SectionApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Obtiene todas las definiciones de secciones del inquilino actual
        /// </summary>
        /// <returns>Lista de definiciones de secciones</returns>
        public async Task<IEnumerable<SectionDefinitionDto>> GetAllSectionDefinitionsAsync()
        {
            var response = await _httpClient.GetAsync("api/tenant-section-definitions");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<IEnumerable<SectionDefinitionDto>>(_jsonOptions) ?? 
                   new List<SectionDefinitionDto>();
        }

        /// <summary>
        /// Obtiene una definición de sección por su ID
        /// </summary>
        /// <param name="sectionId">ID de la definición de sección</param>
        /// <returns>Datos de la definición de sección</returns>
        public async Task<SectionDefinitionDto> GetSectionDefinitionByIdAsync(string sectionId)
        {
            var response = await _httpClient.GetAsync($"api/tenant-section-definitions/{sectionId}");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<SectionDefinitionDto>(_jsonOptions) ?? 
                   new SectionDefinitionDto();
        }

        /// <summary>
        /// Crea una nueva definición de sección
        /// </summary>
        /// <param name="request">Datos de la sección a crear</param>
        /// <returns>Datos de la sección creada</returns>
        public async Task<SectionDefinitionDto> CreateSectionDefinitionAsync(CreateSectionDefinitionRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/tenant-section-definitions", request);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<SectionDefinitionDto>(_jsonOptions) ?? 
                   new SectionDefinitionDto();
        }

        /// <summary>
        /// Actualiza una definición de sección existente
        /// </summary>
        /// <param name="sectionId">ID de la sección a actualizar</param>
        /// <param name="request">Datos a actualizar</param>
        /// <returns>Datos actualizados de la sección</returns>
        public async Task<SectionDefinitionDto> UpdateSectionDefinitionAsync(string sectionId, UpdateSectionDefinitionRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/tenant-section-definitions/{sectionId}", request);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<SectionDefinitionDto>(_jsonOptions) ?? 
                   new SectionDefinitionDto();
        }

        /// <summary>
        /// Elimina una definición de sección
        /// </summary>
        /// <param name="sectionId">ID de la sección a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task DeleteSectionDefinitionAsync(string sectionId)
        {
            var response = await _httpClient.DeleteAsync($"api/tenant-section-definitions/{sectionId}");
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Obtiene todos los campos de una sección
        /// </summary>
        /// <param name="sectionId">ID de la sección</param>
        /// <returns>Lista de definiciones de campos</returns>
        public async Task<IEnumerable<FieldDefinitionDto>> GetFieldDefinitionsBySectionIdAsync(string sectionId)
        {
            var response = await _httpClient.GetAsync($"api/tenant-section-definitions/{sectionId}/fields");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<IEnumerable<FieldDefinitionDto>>(_jsonOptions) ?? 
                   new List<FieldDefinitionDto>();
        }

        /// <summary>
        /// Crea un nuevo campo en una sección
        /// </summary>
        /// <param name="sectionId">ID de la sección</param>
        /// <param name="request">Datos del campo a crear</param>
        /// <returns>Datos del campo creado</returns>
        public async Task<FieldDefinitionDto> CreateFieldDefinitionAsync(string sectionId, CreateFieldDefinitionRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/tenant-section-definitions/{sectionId}/fields", request);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<FieldDefinitionDto>(_jsonOptions) ?? 
                   new FieldDefinitionDto();
        }

        /// <summary>
        /// Actualiza un campo existente
        /// </summary>
        /// <param name="fieldId">ID del campo a actualizar</param>
        /// <param name="request">Datos a actualizar</param>
        /// <returns>Datos actualizados del campo</returns>
        public async Task<FieldDefinitionDto> UpdateFieldDefinitionAsync(string fieldId, UpdateFieldDefinitionRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/tenant-section-definitions/fields/{fieldId}", request);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<FieldDefinitionDto>(_jsonOptions) ?? 
                   new FieldDefinitionDto();
        }

        /// <summary>
        /// Elimina un campo
        /// </summary>
        /// <param name="fieldId">ID del campo a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task DeleteFieldDefinitionAsync(string fieldId)
        {
            var response = await _httpClient.DeleteAsync($"api/tenant-section-definitions/fields/{fieldId}");
            response.EnsureSuccessStatusCode();
        }
    }
} 