using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AppMultiTenant.Application.Configuration;
using AppMultiTenant.Application.Interfaces.Persistence;
using AppMultiTenant.Application.Interfaces.Services;
using AppMultiTenant.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AppMultiTenant.Application.Services
{
    /// <summary>
    /// Implementación del servicio de autenticación que maneja login, registro y refresco de tokens
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantResolverService _tenantResolver;

        /// <summary>
        /// Constructor del servicio de autenticación
        /// </summary>
        /// <param name="userRepository">Repositorio de usuarios</param>
        /// <param name="permissionRepository">Repositorio de permisos</param>
        /// <param name="roleRepository">Repositorio de roles</param>
        /// <param name="userManager">Gestor de usuarios de Identity</param>
        /// <param name="jwtSettings">Configuración de tokens JWT</param>
        /// <param name="unitOfWork">Unit of Work para transacciones</param>
        /// <param name="tenantResolver">Servicio para obtener el inquilino actual</param>
        public AuthService(
            IUserRepository userRepository,
            IPermissionRepository permissionRepository,
            IRoleRepository roleRepository,
            UserManager<ApplicationUser> userManager,
            IOptions<JwtSettings> jwtSettings,
            IUnitOfWork unitOfWork,
            ITenantResolverService tenantResolver)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _tenantResolver = tenantResolver ?? throw new ArgumentNullException(nameof(tenantResolver));
        }

        /// <summary>
        /// Obtiene el ID del inquilino actual del servicio resolutor
        /// </summary>
        /// <returns>ID del inquilino actual</returns>
        /// <exception cref="InvalidOperationException">Si no hay inquilino actual</exception>
        private Guid GetCurrentTenantId()
        {
            var tenantId = _tenantResolver.GetCurrentTenantId();
            if (!tenantId.HasValue)
            {
                throw new InvalidOperationException("No se pudo resolver el inquilino actual. Esta operación requiere un contexto de inquilino.");
            }
            return tenantId.Value;
        }

        #region Métodos con TenantId automático (sobrecargados)

        /// <inheritdoc/>
        public async Task<(string Token, ApplicationUser User)> LoginAsync(string email, string password)
        {
            return await LoginAsync(email, password, GetCurrentTenantId());
        }

        /// <inheritdoc/>
        public async Task<(ApplicationUser User, string Token)> RegisterUserAsync(string userName, string email, string password, string fullName)
        {
            return await RegisterUserAsync(userName, email, password, fullName, GetCurrentTenantId());
        }

        /// <inheritdoc/>
        public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionName)
        {
            return await UserHasPermissionAsync(userId, permissionName, GetCurrentTenantId());
        }

        #endregion

        #region Métodos con TenantId explícito
        
        /// <summary>
        /// Autentica un usuario basado en sus credenciales y genera un token JWT
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>Token JWT y información básica del usuario autenticado, o null si la autenticación falla</returns>
        public async Task<(string Token, ApplicationUser User)> LoginAsync(string email, string password, Guid tenantId)
        {
            if (string.IsNullOrEmpty(email)) throw new ArgumentException("El email no puede estar vacío", nameof(email));
            if (string.IsNullOrEmpty(password)) throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));
            if (tenantId == Guid.Empty) throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));

            // Obtener usuario por email y tenantId
            var user = await _userRepository.GetByEmailAsync(email, tenantId);
            if (user == null)
            {
                return (null, null); // Usuario no encontrado
            }

            // Verificar si el usuario está activo
            if (!user.IsActive)
            {
                return (null, null); // Usuario inactivo
            }

            // Verificar contraseña
            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                // Incrementar contador de intentos fallidos
                await _userManager.AccessFailedAsync(user);
                return (null, null); // Contraseña incorrecta
            }

            // Restablecer contador de intentos fallidos
            if (user.AccessFailedCount > 0)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
            }

            // Registrar login exitoso
            user.RecordLogin();
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Generar token JWT
            var token = await GenerateJwtTokenAsync(user);

            return (token, user);
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema dentro de un inquilino específico
        /// </summary>
        /// <param name="userName">Nombre de usuario</param>
        /// <param name="email">Email del usuario</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <param name="fullName">Nombre completo del usuario</param>
        /// <param name="tenantId">Id del inquilino al que pertenecerá el usuario</param>
        /// <returns>El usuario creado y un token JWT si el registro es exitoso</returns>
        public async Task<(ApplicationUser User, string Token)> RegisterUserAsync(string userName, string email, string password, string fullName, Guid tenantId)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentException("El nombre de usuario no puede estar vacío", nameof(userName));
            if (string.IsNullOrEmpty(email)) throw new ArgumentException("El email no puede estar vacío", nameof(email));
            if (string.IsNullOrEmpty(password)) throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));
            if (string.IsNullOrEmpty(fullName)) throw new ArgumentException("El nombre completo no puede estar vacío", nameof(fullName));
            if (tenantId == Guid.Empty) throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));

            // Verificar si ya existe un usuario con el mismo email en el mismo inquilino
            var existingUser = await _userRepository.GetByEmailAsync(email, tenantId);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"Ya existe un usuario con el email {email} en este inquilino");
            }

            // Crear nuevo usuario
            var user = new ApplicationUser(userName, email, tenantId);
            user.UpdateFullName(fullName);

            // Agregar usuario y establecer contraseña
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Error al crear el usuario: {errors}");
            }

            // Generar token JWT
            var token = await GenerateJwtTokenAsync(user);

            return (user, token);
        }

        /// <summary>
        /// Refresca un token JWT existente
        /// </summary>
        /// <param name="token">Token actual a refrescar</param>
        /// <returns>Nuevo token JWT o null si el token no pudo ser refrescado</returns>
        public async Task<string> RefreshTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("El token no puede estar vacío", nameof(token));
            
            var principal = GetPrincipalFromExpiredToken(token);
            if (principal == null)
                return null;

            // Extraer userId y tenantId del token
            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
            var tenantIdClaim = principal.FindFirst("tenantId");
            
            if (userIdClaim == null || tenantIdClaim == null)
                return null;

            if (!Guid.TryParse(userIdClaim.Value, out var userId) || 
                !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
                return null;

            // Obtener usuario
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive || user.TenantId != tenantId)
                return null;

            // Generar nuevo token
            return await GenerateJwtTokenAsync(user);
        }

        /// <summary>
        /// Valida que un usuario tenga un permiso específico
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <param name="permissionName">Nombre del permiso a verificar</param>
        /// <param name="tenantId">Id del inquilino</param>
        /// <returns>True si el usuario tiene el permiso, false en caso contrario</returns>
        public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionName, Guid tenantId)
        {
            if (userId == Guid.Empty) throw new ArgumentException("El ID del usuario no puede estar vacío", nameof(userId));
            if (string.IsNullOrEmpty(permissionName)) throw new ArgumentException("El nombre del permiso no puede estar vacío", nameof(permissionName));
            if (tenantId == Guid.Empty) throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));

            // Verificar que el usuario exista y pertenezca al inquilino
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.TenantId != tenantId || !user.IsActive)
                return false;

            // Verificar que el permiso exista
            var permission = await _permissionRepository.GetByNameAsync(permissionName);
            if (permission == null)
                return false;

            // Obtener todos los permisos del usuario a través de sus roles
            var userPermissions = await _permissionRepository.GetPermissionsByUserIdAsync(userId);
            
            // Verificar si el permiso buscado está en la lista de permisos del usuario
            return userPermissions.Any(p => p.Id == permission.Id);
        }

        /// <summary>
        /// Cierra la sesión de un usuario revocando sus tokens
        /// </summary>
        /// <param name="userId">Id del usuario</param>
        /// <returns>Task de la operación</returns>
        public async Task LogoutAsync(Guid userId)
        {
            if (userId == Guid.Empty) throw new ArgumentException("El ID del usuario no puede estar vacío", nameof(userId));

            // En una implementación más completa, aquí se revocarían los tokens
            // del usuario, por ejemplo añadiéndolos a una lista negra o
            // cambiando un identificador de seguridad en el usuario.
            
            // Por ahora, solo verificamos que el usuario exista
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("Usuario no encontrado");

            // En futuro, aquí se implementaría el mecanismo de revocación de tokens
            await Task.CompletedTask;
        }

        /// <summary>
        /// Genera un token JWT para un usuario
        /// </summary>
        /// <param name="user">Usuario para el que se genera el token</param>
        /// <returns>Token JWT como string</returns>
        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Obtener roles del usuario
            var userRoles = await _roleRepository.GetRolesByUserIdAsync(user.Id);
            var roleNames = userRoles.Select(r => r.Name).ToList();

            // Agregar claims básicos
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("tenantId", user.TenantId.ToString()),
                new Claim("userName", user.UserName),
                new Claim("fullName", user.FullName ?? string.Empty)
            };

            // Agregar roles como claims
            foreach (var role in roleNames)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Opcionalmente, agregar permisos como claims
            var permissions = await _permissionRepository.GetPermissionsByUserIdAsync(user.Id);
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission.Name));
            }

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Valida un token JWT expirado y extrae su ClaimsPrincipal
        /// </summary>
        /// <param name="token">Token JWT expirado</param>
        /// <returns>ClaimsPrincipal si el token es válido (excepto por la expiración), null en caso contrario</returns>
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = false // No validamos expiración al refrescar
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || 
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }
        
        #endregion
    }
} 