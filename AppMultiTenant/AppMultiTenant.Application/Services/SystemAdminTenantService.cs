using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppMultiTenant.Application.Interfaces.Persistence;
using AppMultiTenant.Application.Interfaces.Services;
using AppMultiTenant.Domain.Entities;

namespace AppMultiTenant.Application.Services
{
    /// <summary>
    /// Implementación del servicio para gestión de inquilinos por parte del Super Administrador
    /// </summary>
    public class SystemAdminTenantService : ISystemAdminTenantService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public SystemAdminTenantService(
            ITenantRepository tenantRepository,
            IUserRepository userRepository,
            IRoleRepository roleRepository)
        {
            _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        }

        /// <inheritdoc />
        public async Task<Tenant> CreateTenantAsync(string name, string identifier)
        {
            // Validar que el identificador esté disponible
            if (!await IsTenantIdentifierAvailableAsync(identifier))
            {
                throw new InvalidOperationException($"El identificador '{identifier}' ya está en uso.");
            }

            // Crear el inquilino
            var tenant = new Tenant(name, identifier);
            
            // Guardarlo en el repositorio
            await _tenantRepository.AddAsync(tenant);
            
            return tenant;
        }

        /// <inheritdoc />
        public async Task<Tenant> GetTenantByIdAsync(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));
            }

            return await _tenantRepository.GetByIdAsync(tenantId);
        }

        /// <inheritdoc />
        public async Task<Tenant> GetTenantByIdentifierAsync(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException("El identificador no puede estar vacío.", nameof(identifier));
            }

            return await _tenantRepository.GetByIdentifierAsync(identifier);
        }

        /// <inheritdoc />
        public async Task<(IEnumerable<Tenant> Tenants, int TotalCount)> GetAllTenantsAsync(bool includeInactive = false, int pageNumber = 1, int pageSize = 20)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentException("El número de página debe ser mayor o igual a 1.", nameof(pageNumber));
            }

            if (pageSize < 1)
            {
                throw new ArgumentException("El tamaño de página debe ser mayor o igual a 1.", nameof(pageSize));
            }

            // Obtener todos los inquilinos
            var allTenants = await _tenantRepository.GetAllAsync();
            
            // Filtrar por estado activo si es necesario
            if (!includeInactive)
            {
                allTenants = allTenants.Where(t => t.IsActive);
            }
            
            // Calcular el total de inquilinos que cumplen con el filtro
            int totalCount = allTenants.Count();
            
            // Aplicar paginación
            var pagedTenants = allTenants
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            return (pagedTenants, totalCount);
        }

        /// <inheritdoc />
        public async Task<Tenant> UpdateTenantNameAsync(Guid tenantId, string name)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("El nombre no puede estar vacío.", nameof(name));
            }

            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                throw new InvalidOperationException($"No se encontró ningún inquilino con el ID {tenantId}.");
            }

            tenant.UpdateName(name);
            await _tenantRepository.UpdateAsync(tenant);
            
            return tenant;
        }

        /// <inheritdoc />
        public async Task<Tenant> UpdateTenantIdentifierAsync(Guid tenantId, string identifier)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));
            }

            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException("El identificador no puede estar vacío.", nameof(identifier));
            }

            // Validar que el nuevo identificador esté disponible (excepto si es el mismo)
            var existingTenant = await _tenantRepository.GetByIdentifierAsync(identifier);
            if (existingTenant != null && existingTenant.TenantId != tenantId)
            {
                throw new InvalidOperationException($"El identificador '{identifier}' ya está en uso.");
            }

            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                throw new InvalidOperationException($"No se encontró ningún inquilino con el ID {tenantId}.");
            }

            tenant.UpdateIdentifier(identifier);
            await _tenantRepository.UpdateAsync(tenant);
            
            return tenant;
        }

        /// <inheritdoc />
        public async Task<Tenant> ActivateTenantAsync(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));
            }

            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                throw new InvalidOperationException($"No se encontró ningún inquilino con el ID {tenantId}.");
            }

            tenant.Activate();
            await _tenantRepository.UpdateAsync(tenant);
            
            return tenant;
        }

        /// <inheritdoc />
        public async Task<Tenant> DeactivateTenantAsync(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));
            }

            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                throw new InvalidOperationException($"No se encontró ningún inquilino con el ID {tenantId}.");
            }

            tenant.Deactivate();
            await _tenantRepository.UpdateAsync(tenant);
            
            return tenant;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteTenantAsync(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));
            }

            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                throw new InvalidOperationException($"No se encontró ningún inquilino con el ID {tenantId}.");
            }

            // Verificar si el inquilino tiene usuarios
            var tenantUsers = await _userRepository.GetAllByTenantIdAsync(tenantId);
            bool hasUsers = tenantUsers.Any();
            
            // Verificar si el inquilino tiene secciones (este método debería implementarse según la estructura real)
            // Para simplificar, asumimos que no hay secciones por ahora
            bool hasSections = false;
            
            // Verificar si el inquilino puede ser eliminado
            if (!tenant.CanBeDeleted(hasUsers, hasSections))
            {
                throw new InvalidOperationException("No se puede eliminar el inquilino porque tiene usuarios o secciones de datos asociadas.");
            }

            // Eliminar el inquilino
            await _tenantRepository.RemoveAsync(tenant);
            
            return true;
        }

        /// <inheritdoc />
        public async Task<ApplicationUser> CreateInitialTenantAdminAsync(Guid tenantId, string adminEmail, string adminUserName, string adminPassword, string adminFullName)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío.", nameof(tenantId));
            }

            if (string.IsNullOrWhiteSpace(adminEmail))
            {
                throw new ArgumentException("El correo electrónico del administrador no puede estar vacío.", nameof(adminEmail));
            }

            if (string.IsNullOrWhiteSpace(adminUserName))
            {
                throw new ArgumentException("El nombre de usuario del administrador no puede estar vacío.", nameof(adminUserName));
            }

            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new ArgumentException("La contraseña del administrador no puede estar vacía.", nameof(adminPassword));
            }

            // Verificar que el inquilino exista
            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                throw new InvalidOperationException($"No se encontró ningún inquilino con el ID {tenantId}.");
            }

            // Verificar que no exista ya un usuario con ese email en ese tenant
            var existingUser = await _userRepository.GetByEmailAsync(adminEmail, tenantId);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"Ya existe un usuario con el correo electrónico {adminEmail} en este inquilino.");
            }

            // Crear el usuario administrador
            var adminUser = new ApplicationUser(adminUserName, adminEmail, tenantId);
            
            // Actualizar el nombre completo si se proporciona
            if (!string.IsNullOrWhiteSpace(adminFullName))
            {
                adminUser.UpdateFullName(adminFullName);
            }
            
            // En una implementación real, aquí se establecería la contraseña utilizando un servicio
            // de Identity, pero como no tenemos acceso directo a ese servicio en esta capa,
            // asumimos que esto se hará después o en otro lugar.
            // Por ahora simplemente guardamos el usuario
            await _userRepository.AddAsync(adminUser);
            
            // Obtener o crear el rol administrador para este tenant
            var adminRole = await _roleRepository.GetByNameAsync("Administrador", tenantId);
            if (adminRole == null)
            {
                adminRole = new ApplicationRole("Administrador", tenantId, "Administrador del inquilino con acceso completo");
                await _roleRepository.AddAsync(adminRole);
            }
            
            // Asignar el rol administrador al usuario
            await _roleRepository.AssignRoleToUserAsync(adminUser.Id, adminRole.Id);
            
            return adminUser;
        }

        /// <inheritdoc />
        public async Task<bool> IsTenantIdentifierAvailableAsync(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException("El identificador no puede estar vacío.", nameof(identifier));
            }

            return await _tenantRepository.IsIdentifierAvailableAsync(identifier);
        }
    }
} 