namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Interfaz para el cliente de API de gestión de roles
    /// </summary>
    public interface IRoleApiClient : IApiClient
    {
        /// <summary>
        /// Obtiene todos los roles del inquilino actual
        /// </summary>
        /// <returns>Lista de roles</returns>
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();

        /// <summary>
        /// Obtiene un rol por su ID
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <returns>Datos del rol o null si no existe</returns>
        Task<RoleDto> GetRoleByIdAsync(Guid roleId);

        /// <summary>
        /// Crea un nuevo rol en el inquilino actual
        /// </summary>
        /// <param name="roleDto">Datos del rol a crear</param>
        /// <returns>Datos del rol creado o null en caso de error</returns>
        Task<RoleDto> CreateRoleAsync(CreateRoleDto roleDto);

        /// <summary>
        /// Actualiza los datos de un rol existente
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="roleDto">Datos actualizados del rol</param>
        /// <returns>Datos del rol actualizado o null en caso de error</returns>
        Task<RoleDto> UpdateRoleAsync(Guid roleId, UpdateRoleDto roleDto);

        /// <summary>
        /// Elimina un rol
        /// </summary>
        /// <param name="roleId">ID del rol a eliminar</param>
        /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
        Task<bool> DeleteRoleAsync(Guid roleId);

        /// <summary>
        /// Obtiene todos los permisos disponibles en el sistema
        /// </summary>
        /// <returns>Lista de permisos</returns>
        Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();

        /// <summary>
        /// Obtiene los permisos asignados a un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <returns>Lista de permisos asignados al rol</returns>
        Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(Guid roleId);

        /// <summary>
        /// Asigna permisos a un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionIds">IDs de los permisos a asignar</param>
        /// <returns>True si se asignaron correctamente, False en caso contrario</returns>
        Task<bool> AssignPermissionsToRoleAsync(Guid roleId, IEnumerable<string> permissionIds);
    }

    /// <summary>
    /// DTO para crear un nuevo rol
    /// </summary>
    public class CreateRoleDto
    {
        /// <summary>
        /// Nombre del rol
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción del rol
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Permisos iniciales a asignar (IDs)
        /// </summary>
        public IEnumerable<string> PermissionIds { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un rol existente
    /// </summary>
    public class UpdateRoleDto
    {
        /// <summary>
        /// Nombre del rol
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción del rol
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// DTO para representar un permiso en la UI
    /// </summary>
    public class PermissionDto
    {
        /// <summary>
        /// ID del permiso
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Nombre del permiso
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción del permiso
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Grupo al que pertenece el permiso (para agrupar en la UI)
        /// </summary>
        public string Group { get; set; }
    }
} 