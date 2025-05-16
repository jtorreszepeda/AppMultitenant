namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Interfaz para el cliente de API de gestión de inquilinos 
    /// (solo para Super Administradores)
    /// </summary>
    public interface ITenantApiClient : IApiClient
    {
        /// <summary>
        /// Obtiene todos los inquilinos del sistema
        /// </summary>
        /// <returns>Lista de inquilinos</returns>
        Task<IEnumerable<TenantDto>> GetAllTenantsAsync();

        /// <summary>
        /// Obtiene un inquilino por su ID
        /// </summary>
        /// <param name="tenantId">ID del inquilino</param>
        /// <returns>Datos del inquilino o null si no existe</returns>
        Task<TenantDto> GetTenantByIdAsync(Guid tenantId);

        /// <summary>
        /// Crea un nuevo inquilino en el sistema
        /// </summary>
        /// <param name="tenantDto">Datos del inquilino a crear</param>
        /// <returns>Datos del inquilino creado o null en caso de error</returns>
        Task<TenantDto> CreateTenantAsync(CreateTenantDto tenantDto);

        /// <summary>
        /// Actualiza los datos de un inquilino existente
        /// </summary>
        /// <param name="tenantId">ID del inquilino</param>
        /// <param name="tenantDto">Datos actualizados del inquilino</param>
        /// <returns>Datos del inquilino actualizado o null en caso de error</returns>
        Task<TenantDto> UpdateTenantAsync(Guid tenantId, UpdateTenantDto tenantDto);

        /// <summary>
        /// Elimina un inquilino
        /// </summary>
        /// <param name="tenantId">ID del inquilino a eliminar</param>
        /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
        Task<bool> DeleteTenantAsync(Guid tenantId);

        /// <summary>
        /// Obtiene información del inquilino actual (para usuarios normales)
        /// </summary>
        /// <returns>Información básica del inquilino actual</returns>
        Task<TenantInfoDto> GetCurrentTenantInfoAsync();
    }

    /// <summary>
    /// DTO para representar un inquilino en la UI
    /// </summary>
    public class TenantDto
    {
        /// <summary>
        /// ID del inquilino
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre del inquilino
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Identificador único del inquilino (para subdominio)
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Indica si el inquilino está activo
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Fecha de creación del inquilino
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// DTO con información resumida del inquilino actual
    /// </summary>
    public class TenantInfoDto
    {
        /// <summary>
        /// ID del inquilino
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre del inquilino
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// DTO para crear un nuevo inquilino
    /// </summary>
    public class CreateTenantDto
    {
        /// <summary>
        /// Nombre del inquilino
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Identificador único del inquilino (para subdominio)
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Datos del administrador inicial del inquilino
        /// </summary>
        public CreateUserDto AdminUser { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un inquilino existente
    /// </summary>
    public class UpdateTenantDto
    {
        /// <summary>
        /// Nombre del inquilino
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Identificador único del inquilino (para subdominio)
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Indica si el inquilino está activo
        /// </summary>
        public bool IsActive { get; set; }
    }
} 