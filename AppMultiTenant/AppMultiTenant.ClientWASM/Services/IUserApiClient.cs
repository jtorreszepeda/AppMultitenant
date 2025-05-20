namespace AppMultiTenant.ClientWASM.Services
{
    /// <summary>
    /// Modelo para representar un usuario en las operaciones del cliente
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Identificador único del usuario
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de usuario
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico del usuario
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Indica si la cuenta de usuario está activa
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Identificador del inquilino al que pertenece el usuario
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// Roles asignados al usuario
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();
    }

    /// <summary>
    /// Modelo para crear un nuevo usuario
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// Nombre de usuario
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico del usuario
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        public string FullName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para actualizar datos de un usuario
    /// </summary>
    public class UpdateUserRequest
    {
        /// <summary>
        /// Nombre de usuario
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico del usuario
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        public string FullName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo de respuesta paginada
    /// </summary>
    /// <typeparam name="T">Tipo de elementos en la colección</typeparam>
    public class PagedResponse<T>
    {
        /// <summary>
        /// Elementos de la página actual
        /// </summary>
        public IEnumerable<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Número total de elementos
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Número de página actual
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Tamaño de página
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Número total de páginas
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <summary>
    /// Interfaz para el servicio cliente que encapsula las operaciones con usuarios
    /// </summary>
    public interface IUserApiClient
    {
        /// <summary>
        /// Obtiene el usuario por su ID
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Datos del usuario</returns>
        Task<UserDto> GetUserByIdAsync(string userId);

        /// <summary>
        /// Obtiene una lista paginada de usuarios del inquilino actual
        /// </summary>
        /// <param name="pageNumber">Número de página (empezando por 1)</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="includeInactive">Incluir usuarios inactivos</param>
        /// <returns>Lista paginada de usuarios</returns>
        Task<PagedResponse<UserDto>> GetUsersAsync(int pageNumber = 1, int pageSize = 10, bool includeInactive = false);

        /// <summary>
        /// Obtiene un usuario por su email
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <returns>Datos del usuario</returns>
        Task<UserDto> GetUserByEmailAsync(string email);

        /// <summary>
        /// Verifica si un email está disponible dentro del inquilino
        /// </summary>
        /// <param name="email">Email a verificar</param>
        /// <param name="excludeUserId">ID de usuario a excluir de la verificación (opcional)</param>
        /// <returns>True si está disponible, False si ya existe</returns>
        Task<bool> CheckEmailAvailabilityAsync(string email, string excludeUserId = null);

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        /// <param name="request">Datos del usuario a crear</param>
        /// <returns>Datos del usuario creado</returns>
        Task<UserDto> CreateUserAsync(CreateUserRequest request);

        /// <summary>
        /// Actualiza los datos de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario a actualizar</param>
        /// <param name="request">Datos a actualizar</param>
        /// <returns>Datos actualizados del usuario</returns>
        Task<UserDto> UpdateUserAsync(string userId, UpdateUserRequest request);

        /// <summary>
        /// Activa la cuenta de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Datos actualizados del usuario</returns>
        Task<UserDto> ActivateUserAsync(string userId);

        /// <summary>
        /// Desactiva la cuenta de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Datos actualizados del usuario</returns>
        Task<UserDto> DeactivateUserAsync(string userId);

        /// <summary>
        /// Elimina un usuario
        /// </summary>
        /// <param name="userId">ID del usuario a eliminar</param>
        /// <param name="currentUserId">ID del usuario que realiza la operación</param>
        /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
        Task<bool> DeleteUserAsync(string userId, string currentUserId);

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="newPassword">Nueva contraseña</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task ChangePasswordAsync(string userId, string newPassword);

        /// <summary>
        /// Obtiene los roles asignados a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Lista de roles asignados al usuario</returns>
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);

        /// <summary>
        /// Asigna un rol a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleId">ID del rol a asignar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task AssignRoleToUserAsync(string userId, string roleId);

        /// <summary>
        /// Asigna múltiples roles a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleIds">IDs de los roles a asignar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task AssignRolesToUserAsync(string userId, IEnumerable<string> roleIds);

        /// <summary>
        /// Revoca un rol de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleId">ID del rol a revocar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task RevokeRoleFromUserAsync(string userId, string roleId);

        /// <summary>
        /// Revoca múltiples roles de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleIds">IDs de los roles a revocar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task RevokeRolesFromUserAsync(string userId, IEnumerable<string> roleIds);
    }
}