using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Implementación del servicio para obtener las secciones disponibles para el usuario actual
    /// </summary>
    public class SectionService : ISectionService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILogger<SectionService> _logger;

        /// <summary>
        /// Constructor del servicio de secciones
        /// </summary>
        /// <param name="httpClient">Cliente HTTP</param>
        /// <param name="authStateProvider">Proveedor de estado de autenticación</param>
        /// <param name="logger">Logger</param>
        public SectionService(
            HttpClient httpClient,
            AuthenticationStateProvider authStateProvider,
            ILogger<SectionService> logger)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<List<SectionInfo>> GetAvailableSectionsAsync()
        {
            try
            {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                if (!authState.User.Identity?.IsAuthenticated == true)
                {
                    return new List<SectionInfo>();
                }

                // Implementación real: obtener desde la API
                // return await _httpClient.GetFromJsonAsync<List<SectionInfo>>("api/sections/available") ?? new List<SectionInfo>();

                // Implementación simulada para desarrollo (hasta implementar la API completa)
                await Task.Delay(100); // Simular retraso de red
                return GetMockSections(authState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las secciones disponibles");
                return new List<SectionInfo>();
            }
        }

        /// <summary>
        /// Método auxiliar para generar secciones de prueba
        /// </summary>
        private List<SectionInfo> GetMockSections(AuthenticationState authState)
        {
            var sections = new List<SectionInfo>
            {
                new SectionInfo
                {
                    Id = "clients",
                    Name = "Clientes",
                    Icon = "people",
                    CanCreate = true,
                    CanEdit = true,
                    CanDelete = authState.User.IsInRole("TenantAdmin")
                },
                new SectionInfo
                {
                    Id = "projects",
                    Name = "Proyectos",
                    Icon = "work",
                    CanCreate = true,
                    CanEdit = true,
                    CanDelete = authState.User.IsInRole("TenantAdmin")
                }
            };

            // Solo para roles específicos
            if (authState.User.IsInRole("DataManager") || authState.User.IsInRole("TenantAdmin"))
            {
                sections.Add(new SectionInfo
                {
                    Id = "reports",
                    Name = "Informes",
                    Icon = "assessment",
                    CanCreate = true,
                    CanEdit = true,
                    CanDelete = authState.User.IsInRole("TenantAdmin")
                });
            }

            return sections;
        }
    }
} 