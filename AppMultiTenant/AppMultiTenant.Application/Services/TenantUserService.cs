using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppMultiTenant.Application.Interfaces.Persistence;
using AppMultiTenant.Application.Interfaces.Services;
using AppMultiTenant.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AppMultiTenant.Application.Services
{
    /// <summary>
    /// Implementación del servicio para la gestión de usuarios dentro de un inquilino
    /// </summary>
    public class TenantUserService : ITenantUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantResolverService _tenantResolver;

        /// <summary>
        /// Constructor del servicio de gestión de usuarios de inquilino
        /// </summary>
        /// <param name="userRepository">Repositorio de usuarios</param>
        /// <param name="roleRepository">Repositorio de roles</param>
        /// <param name="unitOfWork">Unit of Work para transacciones</param>
        /// <param name="userManager">Gestor de usuarios de Identity</param>
        /// <param name="tenantResolver">Servicio para obtener el inquilino actual</param>
        public TenantUserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ITenantResolverService tenantResolver)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
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

        /// <inheritdoc />
        public async Task<ApplicationUser> CreateUserAsync(string userName, string email, string password, string fullName)
        {
            return await CreateUserAsync(userName, email, password, fullName, GetCurrentTenantId());
        }
        
        /// <inheritdoc />
        public async Task<ApplicationUser> GetUserByIdAsync(Guid userId)
        {
            return await GetUserByIdAsync(userId, GetCurrentTenantId());
        }
        
        /// <inheritdoc />
        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await GetUserByEmailAsync(email, GetCurrentTenantId());
        }
        
        /// <inheritdoc />
        public async Task<(IEnumerable<ApplicationUser> Users, int TotalCount)> GetAllUsersAsync(bool includeInactive = false, int pageNumber = 1, int pageSize = 20)
        {
            return await GetAllUsersAsync(GetCurrentTenantId(), includeInactive, pageNumber, pageSize);
        }
        
        /// <inheritdoc />
        public async Task<ApplicationUser> UpdateUserFullNameAsync(Guid userId, string fullName)
        {
            return await UpdateUserFullNameAsync(userId, fullName, GetCurrentTenantId());
        }
        
        /// <inheritdoc />
        public async Task<ApplicationUser> UpdateUserEmailAsync(Guid userId, string email)
        {
            return await UpdateUserEmailAsync(userId, email, GetCurrentTenantId());
        }
        
        /// <inheritdoc />
        public async Task<ApplicationUser> UpdateUserNameAsync(Guid userId, string userName)
        {
            return await UpdateUserNameAsync(userId, userName, GetCurrentTenantId());
        }
        
        /// <inheritdoc />
        public async Task<bool> ChangeUserPasswordAsync(Guid userId, string newPassword, string currentPassword = null)
        {
            return await ChangeUserPasswordAsync(userId, newPassword, currentPassword, GetCurrentTenantId());
        }
        
        /// <inheritdoc />
        public async Task<bool> ResetUserPasswordAsync(Guid userId, string newPassword)
        {
            return await ResetUserPasswordAsync(userId, newPassword, GetCurrentTenantId());
        }
        
        /// <inheritdoc />
        public async Task<ApplicationUser> ActivateUserAsync(Guid userId)
        {
            return await ActivateUserAsync(userId, GetCurrentTenantId());
        }
        
        /// <inheritdoc />
        public async Task<ApplicationUser> DeactivateUserAsync(Guid userId)
        {
            return await DeactivateUserAsync(userId, GetCurrentTenantId());
        }
        
        /// <inheritdoc />
        public async Task<bool> DeleteUserAsync(Guid userId, Guid currentUserId)
        {
            return await DeleteUserAsync(userId, GetCurrentTenantId(), currentUserId);
        }
        
        /// <inheritdoc />
        public async Task<bool> IsEmailAvailableAsync(string email, Guid? excludeUserId = null)
        {
            return await IsEmailAvailableAsync(email, GetCurrentTenantId(), excludeUserId);
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<ApplicationRole>> AssignRolesToUserAsync(Guid userId, IEnumerable<Guid> roleIds)
        {
            return await AssignRolesToUserAsync(userId, roleIds, GetCurrentTenantId());
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<ApplicationRole>> GetUserRolesAsync(Guid userId)
        {
            return await GetUserRolesAsync(userId, GetCurrentTenantId());
        }
        
        /// <inheritdoc />
        public async Task<bool> RemoveRolesFromUserAsync(Guid userId, IEnumerable<Guid> roleIds)
        {
            return await RemoveRolesFromUserAsync(userId, roleIds, GetCurrentTenantId());
        }

        #endregion

        #region Métodos originales con TenantId explícito

        /// <inheritdoc />
        public async Task<ApplicationUser> CreateUserAsync(string userName, string email, string password, string fullName, Guid tenantId)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("El nombre de usuario no puede estar vacío.", nameof(userName));
                
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El correo electrónico no puede estar vacío.", nameof(email));
                
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía.", nameof(password));
                
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));

            // Verificar si el email ya está en uso en este inquilino
            var existingUser = await _userRepository.GetByEmailAsync(email, tenantId);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"Ya existe un usuario con el correo electrónico {email} en este inquilino.");
            }

            // Crear nuevo usuario
            var user = new ApplicationUser(userName, email, tenantId);
            
            // Establecer nombre completo si se proporciona
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                user.UpdateFullName(fullName);
            }

            // Crear usuario con Identity y establecer contraseña
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Error al crear el usuario: {errors}");
            }

            return user;
        }

        /// <inheritdoc />
        public async Task<ApplicationUser> GetUserByIdAsync(Guid userId, Guid tenantId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El ID del usuario no puede estar vacío.", nameof(userId));
                
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));

            var user = await _userRepository.GetByIdAsync(userId);
            
            // Verificar que el usuario exista y pertenezca al inquilino correcto
            if (user == null || user.TenantId != tenantId)
            {
                return null;
            }

            return user;
        }

        /// <inheritdoc />
        public async Task<ApplicationUser> GetUserByEmailAsync(string email, Guid tenantId)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El correo electrónico no puede estar vacío.", nameof(email));
                
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));

            return await _userRepository.GetByEmailAsync(email, tenantId);
        }

        /// <inheritdoc />
        public async Task<(IEnumerable<ApplicationUser> Users, int TotalCount)> GetAllUsersAsync(Guid tenantId, bool includeInactive = false, int pageNumber = 1, int pageSize = 20)
        {
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));
                
            if (pageNumber < 1)
                throw new ArgumentException("El número de página debe ser mayor o igual a 1.", nameof(pageNumber));
                
            if (pageSize < 1)
                throw new ArgumentException("El tamaño de página debe ser mayor o igual a 1.", nameof(pageSize));

            // Obtener usuarios del inquilino
            var allUsers = includeInactive 
                ? await _userRepository.GetAllByTenantIdAsync(tenantId)
                : await _userRepository.GetAllActiveByTenantIdAsync(tenantId);
            
            // Calcular total de usuarios
            int totalCount = allUsers.Count();
            
            // Aplicar paginación
            var pagedUsers = allUsers
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            return (pagedUsers, totalCount);
        }

        /// <inheritdoc />
        public async Task<ApplicationUser> UpdateUserFullNameAsync(Guid userId, string fullName, Guid tenantId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El ID del usuario no puede estar vacío.", nameof(userId));
                
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("El nombre completo no puede estar vacío.", nameof(fullName));
                
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));

            var user = await GetUserByIdAsync(userId, tenantId);
            if (user == null)
            {
                throw new InvalidOperationException($"No se encontró ningún usuario con el ID {userId} en el inquilino {tenantId}.");
            }
            
            user.UpdateFullName(fullName);
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            return user;
        }

        /// <inheritdoc />
        public async Task<ApplicationUser> UpdateUserEmailAsync(Guid userId, string email, Guid tenantId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El ID del usuario no puede estar vacío.", nameof(userId));
                
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El correo electrónico no puede estar vacío.", nameof(email));
                
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));

            // Verificar si el email está disponible (exceptuando el usuario actual)
            if (!await IsEmailAvailableAsync(email, tenantId, userId))
            {
                throw new InvalidOperationException($"El correo electrónico {email} ya está en uso por otro usuario en este inquilino.");
            }

            var user = await GetUserByIdAsync(userId, tenantId);
            if (user == null)
            {
                throw new InvalidOperationException($"No se encontró ningún usuario con el ID {userId} en el inquilino {tenantId}.");
            }
            
            user.UpdateEmail(email);
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            return user;
        }

        /// <inheritdoc />
        public async Task<ApplicationUser> UpdateUserNameAsync(Guid userId, string userName, Guid tenantId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El ID del usuario no puede estar vacío.", nameof(userId));
                
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("El nombre de usuario no puede estar vacío.", nameof(userName));
                
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));

            var user = await GetUserByIdAsync(userId, tenantId);
            if (user == null)
            {
                throw new InvalidOperationException($"No se encontró ningún usuario con el ID {userId} en el inquilino {tenantId}.");
            }
            
            user.UpdateUserName(userName);
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            return user;
        }

        /// <inheritdoc />
        public async Task<bool> ChangeUserPasswordAsync(Guid userId, string newPassword, string currentPassword, Guid tenantId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El ID del usuario no puede estar vacío.", nameof(userId));
                
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("La nueva contraseña no puede estar vacía.", nameof(newPassword));
                
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));

            var user = await GetUserByIdAsync(userId, tenantId);
            if (user == null)
            {
                throw new InvalidOperationException($"No se encontró ningún usuario con el ID {userId} en el inquilino {tenantId}.");
            }

            IdentityResult result;
            
            if (!string.IsNullOrWhiteSpace(currentPassword))
            {
                // Cambio de contraseña con verificación de la contraseña actual
                result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            }
            else
            {
                // Este camino requiere privilegios administrativos y se debería validar en la capa de controlador
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            }
            
            return result.Succeeded;
        }

        /// <inheritdoc />
        public async Task<bool> ResetUserPasswordAsync(Guid userId, string newPassword, Guid tenantId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El ID del usuario no puede estar vacío.", nameof(userId));
                
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("La nueva contraseña no puede estar vacía.", nameof(newPassword));
                
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));

            var user = await GetUserByIdAsync(userId, tenantId);
            if (user == null)
            {
                throw new InvalidOperationException($"No se encontró ningún usuario con el ID {userId} en el inquilino {tenantId}.");
            }
            
            // Generar token de reset y cambiar contraseña
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            
            return result.Succeeded;
        }

        /// <inheritdoc />
        public async Task<ApplicationUser> ActivateUserAsync(Guid userId, Guid tenantId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El ID del usuario no puede estar vacío.", nameof(userId));
                
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));

            var user = await GetUserByIdAsync(userId, tenantId);
            if (user == null)
            {
                throw new InvalidOperationException($"No se encontró ningún usuario con el ID {userId} en el inquilino {tenantId}.");
            }
            
            user.Activate();
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            return user;
        }

        /// <inheritdoc />
        public async Task<ApplicationUser> DeactivateUserAsync(Guid userId, Guid tenantId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El ID del usuario no puede estar vacío.", nameof(userId));
                
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));

            var user = await GetUserByIdAsync(userId, tenantId);
            if (user == null)
            {
                throw new InvalidOperationException($"No se encontró ningún usuario con el ID {userId} en el inquilino {tenantId}.");
            }
            
            user.Deactivate();
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            return user;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteUserAsync(Guid userId, Guid tenantId, Guid currentUserId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El ID del usuario no puede estar vacío.", nameof(userId));
                
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));
                
            if (currentUserId == Guid.Empty)
                throw new ArgumentException("El ID del usuario actual no puede estar vacío.", nameof(currentUserId));

            var user = await GetUserByIdAsync(userId, tenantId);
            if (user == null)
            {
                throw new InvalidOperationException($"No se encontró ningún usuario con el ID {userId} en el inquilino {tenantId}.");
            }
            
            // Verificar si es el usuario actual (no se puede eliminar a sí mismo)
            bool isCurrentUser = userId == currentUserId;
            
            // Verificar si es el último administrador del inquilino
            bool isLastAdminOfTenant = false;
            
            // Solo verificamos si es el último admin si el usuario tiene roles de administrador
            var userRoles = await _roleRepository.GetRolesByUserIdAsync(userId);
            if (userRoles.Any(r => r.Name == "Administrador"))
            {
                // Verificar cuántos administradores hay en total en el inquilino
                bool hasOtherAdmins = await _userRepository.ExistsAdminInTenantAsync(tenantId) 
                    && (await _roleRepository.GetRolesByUserIdAsync(userId))
                        .Any(r => r.Name == "Administrador" && r.TenantId == tenantId);
                
                isLastAdminOfTenant = !hasOtherAdmins;
            }
            
            // Verificar si el usuario puede ser eliminado
            if (!user.CanBeDeleted(isCurrentUser, isLastAdminOfTenant))
            {
                return false;
            }
            
            // Eliminar el usuario
            await _userRepository.RemoveAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> IsEmailAvailableAsync(string email, Guid tenantId, Guid? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("El correo electrónico no puede estar vacío.", nameof(email));
                
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));

            var existingUser = await _userRepository.GetByEmailAsync(email, tenantId);
            
            // Si no hay usuario con ese email o es el usuario excluido, el email está disponible
            return existingUser == null || (excludeUserId.HasValue && existingUser.Id == excludeUserId.Value);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ApplicationRole>> AssignRolesToUserAsync(Guid userId, IEnumerable<Guid> roleIds, Guid tenantId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El ID del usuario no puede estar vacío.", nameof(userId));
                
            if (roleIds == null || !roleIds.Any())
                throw new ArgumentException("La lista de IDs de roles no puede estar vacía.", nameof(roleIds));
                
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));

            var user = await GetUserByIdAsync(userId, tenantId);
            if (user == null)
            {
                throw new InvalidOperationException($"No se encontró ningún usuario con el ID {userId} en el inquilino {tenantId}.");
            }
            
            // Verificar que todos los roles pertenezcan al mismo inquilino
            var allRoles = new List<ApplicationRole>();
            foreach (var roleId in roleIds)
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null || role.TenantId != tenantId)
                {
                    throw new InvalidOperationException($"El rol con ID {roleId} no existe o no pertenece al inquilino {tenantId}.");
                }
                allRoles.Add(role);
            }
            
            // Asignar cada rol al usuario
            foreach (var roleId in roleIds)
            {
                await _roleRepository.AssignRoleToUserAsync(userId, roleId);
            }
            
            await _unitOfWork.SaveChangesAsync();
            
            return allRoles;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ApplicationRole>> GetUserRolesAsync(Guid userId, Guid tenantId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El ID del usuario no puede estar vacío.", nameof(userId));
                
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));

            var user = await GetUserByIdAsync(userId, tenantId);
            if (user == null)
            {
                throw new InvalidOperationException($"No se encontró ningún usuario con el ID {userId} en el inquilino {tenantId}.");
            }
            
            // Obtener roles del usuario
            var roles = await _roleRepository.GetRolesByUserIdAsync(userId);
            
            // Filtrar solo los roles del inquilino correcto (debería ser innecesario si la arquitectura está bien,
            // pero es una precaución adicional)
            return roles.Where(r => r.TenantId == tenantId);
        }

        /// <inheritdoc />
        public async Task<bool> RemoveRolesFromUserAsync(Guid userId, IEnumerable<Guid> roleIds, Guid tenantId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("El ID del usuario no puede estar vacío.", nameof(userId));
                
            if (roleIds == null || !roleIds.Any())
                throw new ArgumentException("La lista de IDs de roles no puede estar vacía.", nameof(roleIds));
                
            if (tenantId == Guid.Empty)
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));

            var user = await GetUserByIdAsync(userId, tenantId);
            if (user == null)
            {
                throw new InvalidOperationException($"No se encontró ningún usuario con el ID {userId} en el inquilino {tenantId}.");
            }
            
            // Verificar si después de quitar estos roles, el usuario perdería todos los roles de administrador
            // y si es el último administrador del inquilino
            var userRoles = await _roleRepository.GetRolesByUserIdAsync(userId);
            bool userHasAdminRole = userRoles.Any(r => r.Name == "Administrador" && r.TenantId == tenantId);
            bool isLastAdmin = userHasAdminRole && !(await _userRepository.ExistsAdminInTenantAsync(tenantId));
            
            // Si es el último administrador y se le está quitando el rol de administrador, no permitir la operación
            if (isLastAdmin && roleIds.Any(roleId => 
                userRoles.Any(r => r.Id == roleId && r.Name == "Administrador" && r.TenantId == tenantId)))
            {
                throw new InvalidOperationException("No se puede quitar el rol de administrador al último administrador del inquilino.");
            }
            
            // Quitar cada rol al usuario
            foreach (var roleId in roleIds)
            {
                await _roleRepository.RemoveRoleFromUserAsync(userId, roleId);
            }
            
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }

        #endregion
    }
} 