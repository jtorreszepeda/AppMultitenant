using System.Text.Json;

namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Interfaz base para todos los clientes API
    /// </summary>
    public interface IApiClient
    {
        /// <summary>
        /// Obtiene las opciones de serialización JSON predeterminadas
        /// </summary>
        JsonSerializerOptions JsonOptions { get; }
    }
} 