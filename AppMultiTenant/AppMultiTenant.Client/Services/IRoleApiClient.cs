using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppMultiTenant.Client.Services
{
    /// <summary>
    /// Modelo para representar un rol en el cliente
    /// </summary>
    public class RoleDto
    {
        /// <summary>
        /// Identificador único del rol
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del rol
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del rol
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Identificador del inquilino al que pertenece el rol
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// Permisos asignados al rol
        /// </summary>
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
    }

    /// <summary>
    /// Modelo para representar un permiso en el cliente
    /// </summary>
    public class PermissionDto
    {
        /// <summary>
        /// Identificador único del permiso
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del permiso
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del permiso
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Categoría del permiso
        /// </summary>
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para crear un nuevo rol
    /// </summary>
    public class CreateRoleRequest
    {
        /// <summary>
        /// Nombre del rol
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del rol
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para actualizar un rol existente
    /// </summary>
    public class UpdateRoleRequest
    {
        /// <summary>
        /// Nombre del rol
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del rol
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interfaz para el servicio cliente que encapsula las operaciones con roles y permisos
    /// </summary>
    public interface IRoleApiClient
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
        /// <returns>Datos del rol</returns>
        Task<RoleDto> GetRoleByIdAsync(string roleId);

        /// <summary>
        /// Crea un nuevo rol
        /// </summary>
        /// <param name="request">Datos del rol a crear</param>
        /// <returns>Datos del rol creado</returns>
        Task<RoleDto> CreateRoleAsync(CreateRoleRequest request);

        /// <summary>
        /// Actualiza un rol existente
        /// </summary>
        /// <param name="roleId">ID del rol a actualizar</param>
        /// <param name="request">Datos a actualizar</param>
        /// <returns>Datos actualizados del rol</returns>
        Task<RoleDto> UpdateRoleAsync(string roleId, UpdateRoleRequest request);

        /// <summary>
        /// Elimina un rol
        /// </summary>
        /// <param name="roleId">ID del rol a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task DeleteRoleAsync(string roleId);

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
        Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(string roleId);

        /// <summary>
        /// Asigna permisos a un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionIds">IDs de los permisos a asignar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task AssignPermissionsToRoleAsync(string roleId, IEnumerable<string> permissionIds);

        /// <summary>
        /// Revoca permisos de un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionIds">IDs de los permisos a revocar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task RevokePermissionsFromRoleAsync(string roleId, IEnumerable<string> permissionIds);
    }
} 