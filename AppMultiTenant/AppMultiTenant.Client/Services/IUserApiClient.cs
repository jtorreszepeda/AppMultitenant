namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Interfaz para el cliente de API de gestión de usuarios
    /// </summary>
    public interface IUserApiClient : IApiClient
    {
        /// <summary>
        /// Obtiene todos los usuarios del inquilino actual
        /// </summary>
        /// <returns>Lista de usuarios</returns>
        Task<IEnumerable<UserDto>> GetAllUsersAsync();

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Datos del usuario o null si no existe</returns>
        Task<UserDto> GetUserByIdAsync(Guid userId);

        /// <summary>
        /// Crea un nuevo usuario en el inquilino actual
        /// </summary>
        /// <param name="userDto">Datos del usuario a crear</param>
        /// <returns>Datos del usuario creado o null en caso de error</returns>
        Task<UserDto> CreateUserAsync(CreateUserDto userDto);

        /// <summary>
        /// Actualiza los datos de un usuario existente
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="userDto">Datos actualizados del usuario</param>
        /// <returns>Datos del usuario actualizado o null en caso de error</returns>
        Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserDto userDto);

        /// <summary>
        /// Elimina un usuario
        /// </summary>
        /// <param name="userId">ID del usuario a eliminar</param>
        /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
        Task<bool> DeleteUserAsync(Guid userId);

        /// <summary>
        /// Asigna roles a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleIds">IDs de los roles a asignar</param>
        /// <returns>True si se asignaron correctamente, False en caso contrario</returns>
        Task<bool> AssignRolesToUserAsync(Guid userId, IEnumerable<Guid> roleIds);

        /// <summary>
        /// Obtiene los roles asignados a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Lista de roles asignados al usuario</returns>
        Task<IEnumerable<RoleDto>> GetUserRolesAsync(Guid userId);
    }

    /// <summary>
    /// DTO para representar un usuario en la UI
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// ID del usuario
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Email del usuario (se usa como nombre de usuario)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Nombre del usuario
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Apellido del usuario
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Nombre completo del usuario (Nombre + Apellido)
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";

        /// <summary>
        /// Indica si el usuario está activo
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Roles asignados al usuario (opcional, puede necesitar cargarse por separado)
        /// </summary>
        public IEnumerable<RoleDto> Roles { get; set; }
    }

    /// <summary>
    /// DTO para crear un nuevo usuario
    /// </summary>
    public class CreateUserDto
    {
        /// <summary>
        /// Email del usuario (se usa como nombre de usuario)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Contraseña temporal del usuario
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Confirmación de la contraseña
        /// </summary>
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Nombre del usuario
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Apellido del usuario
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Roles iniciales a asignar (IDs)
        /// </summary>
        public IEnumerable<Guid> RoleIds { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un usuario existente
    /// </summary>
    public class UpdateUserDto
    {
        /// <summary>
        /// Email del usuario (se usa como nombre de usuario)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Nombre del usuario
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Apellido del usuario
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Indica si el usuario está activo
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Nueva contraseña (opcional, solo si se cambia)
        /// </summary>
        public string NewPassword { get; set; }

        /// <summary>
        /// Confirmación de la nueva contraseña
        /// </summary>
        public string ConfirmNewPassword { get; set; }
    }

    /// <summary>
    /// DTO para representar un rol en la UI
    /// </summary>
    public class RoleDto
    {
        /// <summary>
        /// ID del rol
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre del rol
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción del rol
        /// </summary>
        public string Description { get; set; }
    }
} 