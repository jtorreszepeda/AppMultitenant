using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppMultiTenant.Domain.Entities;

namespace AppMultiTenant.Application.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio que maneja la gestión de usuarios dentro de un inquilino
    /// </summary>
    public interface ITenantUserService
    {
        #region Métodos con TenantId automático
        
        /// <summary>
        /// Crea un nuevo usuario dentro del inquilino actual
        /// </summary>
        /// <param name="userName">Nombre de usuario</param>
        /// <param name="email">Email del usuario</param>
        /// <param name="password">Contraseña inicial</param>
        /// <param name="fullName">Nombre completo</param>
        /// <returns>El usuario creado</returns>
        Task<ApplicationUser> CreateUserAsync(string userName, string email, string password, string fullName);
        
        /// <summary>
        /// Obtiene un usuario por su Id, verificando que pertenezca al inquilino actual
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <returns>El usuario si existe en el inquilino, null en caso contrario</returns>
        Task<ApplicationUser> GetUserByIdAsync(Guid userId);
        
        /// <summary>
        /// Obtiene un usuario por su email, verificando que pertenezca al inquilino actual
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <returns>El usuario si existe en el inquilino, null en caso contrario</returns>
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        
        /// <summary>
        /// Obtiene todos los usuarios del inquilino actual
        /// </summary>
        /// <param name="includeInactive">Indica si se deben incluir usuarios inactivos</param>
        /// <param name="pageNumber">Número de página para paginación</param>
        /// <param name="pageSize">Tamaño de página para paginación</param>
        /// <returns>Lista paginada de usuarios del inquilino</returns>
        Task<(IEnumerable<ApplicationUser> Users, int TotalCount)> GetAllUsersAsync(
            bool includeInactive = false, int pageNumber = 1, int pageSize = 20);
        
        /// <summary>
        /// Actualiza el nombre completo de un usuario del inquilino actual
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="fullName">Nuevo nombre completo</param>
        /// <returns>El usuario actualizado</returns>
        Task<ApplicationUser> UpdateUserFullNameAsync(Guid userId, string fullName);
        
        /// <summary>
        /// Actualiza el email de un usuario del inquilino actual
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="email">Nuevo email</param>
        /// <returns>El usuario actualizado</returns>
        Task<ApplicationUser> UpdateUserEmailAsync(Guid userId, string email);
        
        /// <summary>
        /// Actualiza el nombre de un usuario del inquilino actual
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="userName">Nuevo nombre de usuario</param>
        /// <returns>El usuario actualizado</returns>
        Task<ApplicationUser> UpdateUserNameAsync(Guid userId, string userName);
        
        /// <summary>
        /// Cambia la contraseña de un usuario del inquilino actual
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="newPassword">Nueva contraseña</param>
        /// <param name="currentPassword">Contraseña actual (opcional, si se requiere validación)</param>
        /// <returns>True si el cambio fue exitoso, false en caso contrario</returns>
        Task<bool> ChangeUserPasswordAsync(Guid userId, string newPassword, string currentPassword = null);
        
        /// <summary>
        /// Resetea la contraseña de un usuario del inquilino actual (por administrador)
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="newPassword">Nueva contraseña</param>
        /// <returns>True si el reset fue exitoso, false en caso contrario</returns>
        Task<bool> ResetUserPasswordAsync(Guid userId, string newPassword);
        
        /// <summary>
        /// Activa un usuario del inquilino actual
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <returns>El usuario actualizado</returns>
        Task<ApplicationUser> ActivateUserAsync(Guid userId);
        
        /// <summary>
        /// Desactiva un usuario del inquilino actual
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <returns>El usuario actualizado</returns>
        Task<ApplicationUser> DeactivateUserAsync(Guid userId);
        
        /// <summary>
        /// Elimina un usuario del inquilino actual (si es posible)
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="currentUserId">Id del usuario que realiza la operación</param>
        /// <returns>True si se eliminó correctamente, false si no se pudo eliminar</returns>
        Task<bool> DeleteUserAsync(Guid userId, Guid currentUserId);
        
        /// <summary>
        /// Verifica si un email está disponible dentro del inquilino actual
        /// </summary>
        /// <param name="email">Email a verificar</param>
        /// <param name="excludeUserId">Id de usuario a excluir de la verificación (para ediciones)</param>
        /// <returns>True si el email está disponible, false si ya está en uso</returns>
        Task<bool> IsEmailAvailableAsync(string email, Guid? excludeUserId = null);
        
        /// <summary>
        /// Asigna roles a un usuario del inquilino actual
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="roleIds">Lista de Ids de roles a asignar</param>
        /// <returns>Lista de roles asignados</returns>
        Task<IEnumerable<ApplicationRole>> AssignRolesToUserAsync(Guid userId, IEnumerable<Guid> roleIds);
        
        /// <summary>
        /// Obtiene los roles asignados a un usuario del inquilino actual
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <returns>Lista de roles asignados al usuario</returns>
        Task<IEnumerable<ApplicationRole>> GetUserRolesAsync(Guid userId);
        
        /// <summary>
        /// Remueve roles de un usuario del inquilino actual
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="roleIds">Lista de Ids de roles a remover</param>
        /// <returns>True si la operación fue exitosa</returns>
        Task<bool> RemoveRolesFromUserAsync(Guid userId, IEnumerable<Guid> roleIds);
        
        #endregion
        
        #region Métodos con TenantId explícito

        /// <summary>
        /// Crea un nuevo usuario dentro del inquilino
        /// </summary>
        /// <param name="userName">Nombre de usuario</param>
        /// <param name="email">Email del usuario</param>
        /// <param name="password">Contraseña inicial</param>
        /// <param name="fullName">Nombre completo</param>
        /// <param name="tenantId">Id del inquilino al que pertenecerá</param>
        /// <returns>El usuario creado</returns>
        Task<ApplicationUser> CreateUserAsync(string userName, string email, string password, string fullName, Guid tenantId);
        
        /// <summary>
        /// Obtiene un usuario por su Id, verificando que pertenezca al inquilino
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>El usuario si existe en el inquilino, null en caso contrario</returns>
        Task<ApplicationUser> GetUserByIdAsync(Guid userId, Guid tenantId);
        
        /// <summary>
        /// Obtiene un usuario por su email, verificando que pertenezca al inquilino
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>El usuario si existe en el inquilino, null en caso contrario</returns>
        Task<ApplicationUser> GetUserByEmailAsync(string email, Guid tenantId);
        
        /// <summary>
        /// Obtiene todos los usuarios de un inquilino
        /// </summary>
        /// <param name="tenantId">Id del inquilino</param>
        /// <param name="includeInactive">Indica si se deben incluir usuarios inactivos</param>
        /// <param name="pageNumber">Número de página para paginación</param>
        /// <param name="pageSize">Tamaño de página para paginación</param>
        /// <returns>Lista paginada de usuarios del inquilino</returns>
        Task<(IEnumerable<ApplicationUser> Users, int TotalCount)> GetAllUsersAsync(Guid tenantId, bool includeInactive = false, int pageNumber = 1, int pageSize = 20);
        
        /// <summary>
        /// Actualiza el nombre completo de un usuario
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="fullName">Nuevo nombre completo</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>El usuario actualizado</returns>
        Task<ApplicationUser> UpdateUserFullNameAsync(Guid userId, string fullName, Guid tenantId);
        
        /// <summary>
        /// Actualiza el email de un usuario
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="email">Nuevo email</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>El usuario actualizado</returns>
        Task<ApplicationUser> UpdateUserEmailAsync(Guid userId, string email, Guid tenantId);
        
        /// <summary>
        /// Actualiza el nombre de usuario
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="userName">Nuevo nombre de usuario</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>El usuario actualizado</returns>
        Task<ApplicationUser> UpdateUserNameAsync(Guid userId, string userName, Guid tenantId);
        
        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="newPassword">Nueva contraseña</param>
        /// <param name="currentPassword">Contraseña actual (opcional, si se requiere validación)</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>True si el cambio fue exitoso, false en caso contrario</returns>
        Task<bool> ChangeUserPasswordAsync(Guid userId, string newPassword, string currentPassword, Guid tenantId);
        
        /// <summary>
        /// Resetea la contraseña de un usuario (por administrador)
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="newPassword">Nueva contraseña</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>True si el reset fue exitoso, false en caso contrario</returns>
        Task<bool> ResetUserPasswordAsync(Guid userId, string newPassword, Guid tenantId);
        
        /// <summary>
        /// Activa un usuario
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>El usuario actualizado</returns>
        Task<ApplicationUser> ActivateUserAsync(Guid userId, Guid tenantId);
        
        /// <summary>
        /// Desactiva un usuario
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>El usuario actualizado</returns>
        Task<ApplicationUser> DeactivateUserAsync(Guid userId, Guid tenantId);
        
        /// <summary>
        /// Elimina un usuario (si es posible)
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <param name="currentUserId">Id del usuario que realiza la operación</param>
        /// <returns>True si se eliminó correctamente, false si no se pudo eliminar</returns>
        Task<bool> DeleteUserAsync(Guid userId, Guid tenantId, Guid currentUserId);
        
        /// <summary>
        /// Verifica si un email está disponible dentro del inquilino
        /// </summary>
        /// <param name="email">Email a verificar</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <param name="excludeUserId">Id de usuario a excluir de la verificación (para ediciones)</param>
        /// <returns>True si el email está disponible, false si ya está en uso</returns>
        Task<bool> IsEmailAvailableAsync(string email, Guid tenantId, Guid? excludeUserId = null);
        
        /// <summary>
        /// Asigna roles a un usuario
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="roleIds">Lista de Ids de roles a asignar</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>Lista de roles asignados</returns>
        Task<IEnumerable<ApplicationRole>> AssignRolesToUserAsync(Guid userId, IEnumerable<Guid> roleIds, Guid tenantId);
        
        /// <summary>
        /// Obtiene los roles asignados a un usuario
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>Lista de roles asignados al usuario</returns>
        Task<IEnumerable<ApplicationRole>> GetUserRolesAsync(Guid userId, Guid tenantId);
        
        /// <summary>
        /// Remueve roles de un usuario
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="roleIds">Lista de Ids de roles a remover</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>True si la operación fue exitosa</returns>
        Task<bool> RemoveRolesFromUserAsync(Guid userId, IEnumerable<Guid> roleIds, Guid tenantId);
        
        #endregion
    }
} 