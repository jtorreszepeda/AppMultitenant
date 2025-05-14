using System;
using System.Threading.Tasks;
using AppMultiTenant.Domain.Entities;

namespace AppMultiTenant.Application.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de autenticación que maneja inicio de sesión, registro y refresco de tokens
    /// </summary>
    public interface IAuthService
    {
        #region Métodos con TenantId automático
        
        /// <summary>
        /// Autentica un usuario basado en sus credenciales y genera un token JWT, usando el inquilino actual
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <returns>Token JWT y información básica del usuario autenticado, o null si la autenticación falla</returns>
        Task<(string Token, ApplicationUser User)> LoginAsync(string email, string password);

        /// <summary>
        /// Registra un nuevo usuario en el sistema dentro del inquilino actual
        /// </summary>
        /// <param name="userName">Nombre de usuario</param>
        /// <param name="email">Email del usuario</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <param name="fullName">Nombre completo del usuario</param>
        /// <returns>El usuario creado y un token JWT si el registro es exitoso</returns>
        Task<(ApplicationUser User, string Token)> RegisterUserAsync(string userName, string email, string password, string fullName);

        /// <summary>
        /// Valida que un usuario tenga un permiso específico en el inquilino actual
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="permissionName">Nombre del permiso a verificar</param>
        /// <returns>True si el usuario tiene el permiso, false en caso contrario</returns>
        Task<bool> UserHasPermissionAsync(Guid userId, string permissionName);
        
        #endregion
        
        #region Métodos con TenantId explícito
        
        /// <summary>
        /// Autentica un usuario basado en sus credenciales y genera un token JWT
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>Token JWT y información básica del usuario autenticado, o null si la autenticación falla</returns>
        Task<(string Token, ApplicationUser User)> LoginAsync(string email, string password, Guid tenantId);

        /// <summary>
        /// Registra un nuevo usuario en el sistema dentro de un inquilino específico
        /// </summary>
        /// <param name="userName">Nombre de usuario</param>
        /// <param name="email">Email del usuario</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <param name="fullName">Nombre completo del usuario</param>
        /// <param name="tenantId">Id del inquilino al que pertenecerá el usuario</param>
        /// <returns>El usuario creado y un token JWT si el registro es exitoso</returns>
        Task<(ApplicationUser User, string Token)> RegisterUserAsync(string userName, string email, string password, string fullName, Guid tenantId);

        /// <summary>
        /// Refresca un token JWT existente
        /// </summary>
        /// <param name="token">Token actual a refrescar</param>
        /// <returns>Nuevo token JWT o null si el token no pudo ser refrescado</returns>
        Task<string> RefreshTokenAsync(string token);

        /// <summary>
        /// Valida que un usuario tenga un permiso específico
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="permissionName">Nombre del permiso a verificar</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>True si el usuario tiene el permiso, false en caso contrario</returns>
        Task<bool> UserHasPermissionAsync(Guid userId, string permissionName, Guid tenantId);

        /// <summary>
        /// Cierra la sesión de un usuario revocando sus tokens
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <returns>Task de la operación</returns>
        Task LogoutAsync(Guid userId);
        
        #endregion
    }
} 