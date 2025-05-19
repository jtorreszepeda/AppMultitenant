using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Modelo para representar un inquilino en el cliente
    /// </summary>
    public class TenantDto
    {
        /// <summary>
        /// Identificador único del inquilino
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del inquilino
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Identificador único del inquilino para acceso (ej. para subdominio)
        /// </summary>
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el inquilino está activo
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Fecha de creación del inquilino
        /// </summary>
        public string CreatedDate { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para crear un nuevo inquilino
    /// </summary>
    public class CreateTenantRequest
    {
        /// <summary>
        /// Nombre del inquilino
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Identificador único del inquilino para acceso (ej. para subdominio)
        /// </summary>
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el inquilino está activo inicialmente
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Datos del administrador inicial del inquilino
        /// </summary>
        public AdminUserInfo AdminUser { get; set; } = new AdminUserInfo();
    }

    /// <summary>
    /// Modelo para actualizar un inquilino existente
    /// </summary>
    public class UpdateTenantRequest
    {
        /// <summary>
        /// Nombre del inquilino
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Identificador único del inquilino para acceso (ej. para subdominio)
        /// </summary>
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el inquilino está activo
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Información del administrador inicial del inquilino
    /// </summary>
    public class AdminUserInfo
    {
        /// <summary>
        /// Nombre de usuario del administrador
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico del administrador
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña inicial del administrador
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del administrador
        /// </summary>
        public string FullName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interfaz para el servicio cliente que encapsula las operaciones con inquilinos
    /// (para Super Administradores)
    /// </summary>
    public interface ITenantApiClient
    {
        /// <summary>
        /// Obtiene todos los inquilinos
        /// </summary>
        /// <returns>Lista de inquilinos</returns>
        Task<IEnumerable<TenantDto>> GetAllTenantsAsync();

        /// <summary>
        /// Obtiene un inquilino por su ID
        /// </summary>
        /// <param name="tenantId">ID del inquilino</param>
        /// <returns>Datos del inquilino</returns>
        Task<TenantDto> GetTenantByIdAsync(string tenantId);

        /// <summary>
        /// Crea un nuevo inquilino
        /// </summary>
        /// <param name="request">Datos del inquilino a crear</param>
        /// <returns>Datos del inquilino creado</returns>
        Task<TenantDto> CreateTenantAsync(CreateTenantRequest request);

        /// <summary>
        /// Actualiza un inquilino existente
        /// </summary>
        /// <param name="tenantId">ID del inquilino a actualizar</param>
        /// <param name="request">Datos a actualizar</param>
        /// <returns>Datos actualizados del inquilino</returns>
        Task<TenantDto> UpdateTenantAsync(string tenantId, UpdateTenantRequest request);

        /// <summary>
        /// Activa un inquilino
        /// </summary>
        /// <param name="tenantId">ID del inquilino</param>
        /// <returns>Datos actualizados del inquilino</returns>
        Task<TenantDto> ActivateTenantAsync(string tenantId);

        /// <summary>
        /// Desactiva un inquilino
        /// </summary>
        /// <param name="tenantId">ID del inquilino</param>
        /// <returns>Datos actualizados del inquilino</returns>
        Task<TenantDto> DeactivateTenantAsync(string tenantId);

        /// <summary>
        /// Elimina un inquilino (operación peligrosa)
        /// </summary>
        /// <param name="tenantId">ID del inquilino a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task DeleteTenantAsync(string tenantId);

        /// <summary>
        /// Obtiene estadísticas básicas de un inquilino
        /// </summary>
        /// <param name="tenantId">ID del inquilino</param>
        /// <returns>Estadísticas del inquilino</returns>
        Task<TenantStatistics> GetTenantStatisticsAsync(string tenantId);
    }

    /// <summary>
    /// Estadísticas de un inquilino
    /// </summary>
    public class TenantStatistics
    {
        /// <summary>
        /// Número total de usuarios en el inquilino
        /// </summary>
        public int TotalUsers { get; set; }

        /// <summary>
        /// Número de usuarios activos en el inquilino
        /// </summary>
        public int ActiveUsers { get; set; }

        /// <summary>
        /// Número de roles definidos en el inquilino
        /// </summary>
        public int TotalRoles { get; set; }

        /// <summary>
        /// Número de secciones de datos definidas en el inquilino
        /// </summary>
        public int TotalSections { get; set; }
    }
} 