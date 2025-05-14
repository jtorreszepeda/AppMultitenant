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
    /// Implementación del servicio que maneja la gestión de definiciones de secciones dentro de un inquilino
    /// </summary>
    public class TenantSectionDefinitionService : ITenantSectionDefinitionService
    {
        private readonly IAppSectionDefinitionRepository _sectionDefinitionRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantResolverService _tenantResolver;

        /// <summary>
        /// Constructor con inyección de dependencias
        /// </summary>
        /// <param name="sectionDefinitionRepository">Repositorio de definiciones de secciones</param>
        /// <param name="permissionRepository">Repositorio de permisos</param>
        /// <param name="roleRepository">Repositorio de roles</param>
        /// <param name="unitOfWork">Unidad de trabajo para transacciones</param>
        /// <param name="tenantResolver">Servicio para obtener el inquilino actual</param>
        public TenantSectionDefinitionService(
            IAppSectionDefinitionRepository sectionDefinitionRepository,
            IPermissionRepository permissionRepository,
            IRoleRepository roleRepository,
            IUnitOfWork unitOfWork,
            ITenantResolverService tenantResolver)
        {
            _sectionDefinitionRepository = sectionDefinitionRepository ?? throw new ArgumentNullException(nameof(sectionDefinitionRepository));
            _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
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
        public async Task<AppSectionDefinition> CreateSectionDefinitionAsync(string name, string description)
        {
            return await CreateSectionDefinitionAsync(name, description, GetCurrentTenantId());
        }

        /// <inheritdoc/>
        public async Task<AppSectionDefinition> GetSectionDefinitionByIdAsync(Guid sectionDefinitionId)
        {
            return await GetSectionDefinitionByIdAsync(sectionDefinitionId, GetCurrentTenantId());
        }

        /// <inheritdoc/>
        public async Task<AppSectionDefinition> GetSectionDefinitionByNameAsync(string name)
        {
            return await GetSectionDefinitionByNameAsync(name, GetCurrentTenantId());
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AppSectionDefinition>> GetAllSectionDefinitionsAsync()
        {
            return await GetAllSectionDefinitionsAsync(GetCurrentTenantId());
        }

        /// <inheritdoc/>
        public async Task<AppSectionDefinition> UpdateSectionDefinitionNameAsync(Guid sectionDefinitionId, string name)
        {
            return await UpdateSectionDefinitionNameAsync(sectionDefinitionId, name, GetCurrentTenantId());
        }

        /// <inheritdoc/>
        public async Task<AppSectionDefinition> UpdateSectionDefinitionDescriptionAsync(Guid sectionDefinitionId, string description)
        {
            return await UpdateSectionDefinitionDescriptionAsync(sectionDefinitionId, description, GetCurrentTenantId());
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteSectionDefinitionAsync(Guid sectionDefinitionId, bool force = false)
        {
            return await DeleteSectionDefinitionAsync(sectionDefinitionId, GetCurrentTenantId(), force);
        }

        /// <inheritdoc/>
        public async Task<bool> IsSectionNameAvailableAsync(string name, Guid? excludeSectionDefinitionId = null)
        {
            return await IsSectionNameAvailableAsync(name, GetCurrentTenantId(), excludeSectionDefinitionId);
        }

        /// <inheritdoc/>
        public async Task<bool> CanDeleteSectionDefinitionAsync(Guid sectionDefinitionId)
        {
            return await CanDeleteSectionDefinitionAsync(sectionDefinitionId, GetCurrentTenantId());
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Permission>> CreateAndAssignSectionPermissionsAsync(
            Guid sectionDefinitionId, string sectionName, Guid adminRoleId)
        {
            return await CreateAndAssignSectionPermissionsAsync(
                sectionDefinitionId, sectionName, adminRoleId, GetCurrentTenantId());
        }

        #endregion

        #region Métodos con TenantId explícito

        /// <inheritdoc/>
        public async Task<AppSectionDefinition> CreateSectionDefinitionAsync(string name, string description, Guid tenantId)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("El nombre de la sección no puede estar vacío", nameof(name));
            }

            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));
            }

            // Verificar que el nombre no esté en uso
            bool isNameAvailable = await IsSectionNameAvailableAsync(name, tenantId);
            if (!isNameAvailable)
            {
                throw new InvalidOperationException($"El nombre de sección '{name}' ya está en uso en este inquilino");
            }

            // Crear la nueva definición de sección
            var newSectionDefinition = new AppSectionDefinition(tenantId, name, description);
            
            // Agregar al repositorio
            await _sectionDefinitionRepository.AddAsync(newSectionDefinition);
            await _unitOfWork.SaveChangesAsync();
            
            return newSectionDefinition;
        }

        /// <inheritdoc/>
        public async Task<AppSectionDefinition> GetSectionDefinitionByIdAsync(Guid sectionDefinitionId, Guid tenantId)
        {
            if (sectionDefinitionId == Guid.Empty)
            {
                throw new ArgumentException("El ID de la definición de sección no puede estar vacío", nameof(sectionDefinitionId));
            }

            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));
            }

            var sectionDefinition = await _sectionDefinitionRepository.GetByIdAsync(sectionDefinitionId);
            
            // Verificar que la definición de sección pertenezca al inquilino
            if (sectionDefinition == null || sectionDefinition.TenantId != tenantId)
            {
                return null;
            }
            
            return sectionDefinition;
        }

        /// <inheritdoc/>
        public async Task<AppSectionDefinition> GetSectionDefinitionByNameAsync(string name, Guid tenantId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("El nombre de la sección no puede estar vacío", nameof(name));
            }

            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));
            }

            return await _sectionDefinitionRepository.GetByNameAsync(name, tenantId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AppSectionDefinition>> GetAllSectionDefinitionsAsync(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));
            }

            return await _sectionDefinitionRepository.GetAllByTenantIdAsync(tenantId);
        }

        /// <inheritdoc/>
        public async Task<AppSectionDefinition> UpdateSectionDefinitionNameAsync(Guid sectionDefinitionId, string name, Guid tenantId)
        {
            // Validaciones
            if (sectionDefinitionId == Guid.Empty)
            {
                throw new ArgumentException("El ID de la definición de sección no puede estar vacío", nameof(sectionDefinitionId));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("El nombre de la sección no puede estar vacío", nameof(name));
            }

            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));
            }

            // Buscar la definición de sección
            var sectionDefinition = await GetSectionDefinitionByIdAsync(sectionDefinitionId, tenantId);
            if (sectionDefinition == null)
            {
                throw new InvalidOperationException($"No se encontró ninguna definición de sección con ID {sectionDefinitionId} en el inquilino {tenantId}");
            }

            // Si el nombre no cambió, devolver la sección sin modificaciones
            if (sectionDefinition.Name == name)
            {
                return sectionDefinition;
            }

            // Verificar que el nombre no esté en uso
            bool isNameAvailable = await IsSectionNameAvailableAsync(name, tenantId, sectionDefinitionId);
            if (!isNameAvailable)
            {
                throw new InvalidOperationException($"El nombre de sección '{name}' ya está en uso en este inquilino");
            }

            // Actualizar el nombre
            sectionDefinition.UpdateName(name);
            await _unitOfWork.SaveChangesAsync();
            
            return sectionDefinition;
        }

        /// <inheritdoc/>
        public async Task<AppSectionDefinition> UpdateSectionDefinitionDescriptionAsync(Guid sectionDefinitionId, string description, Guid tenantId)
        {
            // Validaciones
            if (sectionDefinitionId == Guid.Empty)
            {
                throw new ArgumentException("El ID de la definición de sección no puede estar vacío", nameof(sectionDefinitionId));
            }

            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));
            }

            // Buscar la definición de sección
            var sectionDefinition = await GetSectionDefinitionByIdAsync(sectionDefinitionId, tenantId);
            if (sectionDefinition == null)
            {
                throw new InvalidOperationException($"No se encontró ninguna definición de sección con ID {sectionDefinitionId} en el inquilino {tenantId}");
            }

            // Actualizar la descripción
            sectionDefinition.UpdateDescription(description);
            await _unitOfWork.SaveChangesAsync();
            
            return sectionDefinition;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteSectionDefinitionAsync(Guid sectionDefinitionId, Guid tenantId, bool force = false)
        {
            // Validaciones
            if (sectionDefinitionId == Guid.Empty)
            {
                throw new ArgumentException("El ID de la definición de sección no puede estar vacío", nameof(sectionDefinitionId));
            }

            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));
            }

            // Buscar la definición de sección
            var sectionDefinition = await GetSectionDefinitionByIdAsync(sectionDefinitionId, tenantId);
            if (sectionDefinition == null)
            {
                return false; // No se encontró la sección, no hay nada que eliminar
            }

            // Verificar si se puede eliminar
            if (!force)
            {
                bool canDelete = await CanDeleteSectionDefinitionAsync(sectionDefinitionId, tenantId);
                if (!canDelete)
                {
                    return false; // No se puede eliminar (probablemente tiene datos asociados)
                }
            }

            // Eliminar la definición
            await _sectionDefinitionRepository.RemoveAsync(sectionDefinition);
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> IsSectionNameAvailableAsync(string name, Guid tenantId, Guid? excludeSectionDefinitionId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("El nombre de la sección no puede estar vacío", nameof(name));
            }

            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));
            }

            // Verificar si existe una sección con el mismo nombre
            if (excludeSectionDefinitionId.HasValue)
            {
                // Si estamos verificando para una edición, excluimos la sección actual
                var existingSection = await _sectionDefinitionRepository.GetByNameAsync(name, tenantId);
                return existingSection == null || existingSection.Id == excludeSectionDefinitionId.Value;
            }
            else
            {
                // Si estamos verificando para una nueva sección, simplemente comprobamos si existe
                return !await _sectionDefinitionRepository.ExistsByNameInTenantAsync(name, tenantId);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CanDeleteSectionDefinitionAsync(Guid sectionDefinitionId, Guid tenantId)
        {
            // Validaciones
            if (sectionDefinitionId == Guid.Empty)
            {
                throw new ArgumentException("El ID de la definición de sección no puede estar vacío", nameof(sectionDefinitionId));
            }

            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));
            }

            // Buscar la definición de sección
            var sectionDefinition = await GetSectionDefinitionByIdAsync(sectionDefinitionId, tenantId);
            if (sectionDefinition == null)
            {
                return false; // No se encontró la sección, no hay nada que eliminar
            }

            // Aquí se verificaría si hay datos asociados a esta sección
            // Por ejemplo, consultando tablas de datos asociadas
            // Por ahora, simplemente asumimos que se puede eliminar si no hay una implementación específica
            
            // En una implementación real, podríamos hacer algo como:
            // bool hasData = await _sectionDataRepository.HasDataForSectionAsync(sectionDefinitionId);
            // return sectionDefinition.CanBeDeleted(hasData);
            
            // Por ahora, utilizamos CanBeDeleted con un valor fijo (false) para simular que no hay datos
            return sectionDefinition.CanBeDeleted(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Permission>> CreateAndAssignSectionPermissionsAsync(
            Guid sectionDefinitionId, string sectionName, Guid adminRoleId, Guid tenantId)
        {
            // Validaciones
            if (sectionDefinitionId == Guid.Empty)
            {
                throw new ArgumentException("El ID de la definición de sección no puede estar vacío", nameof(sectionDefinitionId));
            }

            if (string.IsNullOrWhiteSpace(sectionName))
            {
                throw new ArgumentException("El nombre de la sección no puede estar vacío", nameof(sectionName));
            }

            if (adminRoleId == Guid.Empty)
            {
                throw new ArgumentException("El ID del rol de administrador no puede estar vacío", nameof(adminRoleId));
            }

            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("El ID del inquilino no puede estar vacío", nameof(tenantId));
            }

            // Verificar que la sección exista
            var sectionDefinition = await GetSectionDefinitionByIdAsync(sectionDefinitionId, tenantId);
            if (sectionDefinition == null)
            {
                throw new InvalidOperationException($"No se encontró ninguna definición de sección con ID {sectionDefinitionId} en el inquilino {tenantId}");
            }

            // Verificar que el rol de administrador exista y pertenezca al inquilino
            var adminRole = await _roleRepository.GetByIdAsync(adminRoleId);
            if (adminRole == null || adminRole.TenantId != tenantId)
            {
                throw new InvalidOperationException($"El rol de administrador con ID {adminRoleId} no existe o no pertenece al inquilino {tenantId}");
            }

            // Comenzar una transacción para asegurar la consistencia
            using (await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    // Crear los permisos para la sección
                    var sectionPermissions = Permission.CreateSectionPermissions(sectionName);
                    var createdPermissions = new List<Permission>();

                    // Guardar cada permiso y asignarlo al rol de administrador
                    foreach (var permission in sectionPermissions)
                    {
                        // Verificar si el permiso ya existe
                        var existingPermission = await _permissionRepository.GetByNameAsync(permission.Name);
                        if (existingPermission == null)
                        {
                            // Si no existe, guardarlo
                            await _permissionRepository.AddAsync(permission);
                            await _unitOfWork.SaveChangesAsync();
                            existingPermission = permission;
                        }

                        createdPermissions.Add(existingPermission);

                        // Asignar el permiso al rol de administrador
                        await _permissionRepository.AssignPermissionToRoleAsync(adminRoleId, existingPermission.Id);
                    }

                    // Confirmar la transacción
                    await _unitOfWork.CommitTransactionAsync();

                    return createdPermissions;
                }
                catch (Exception)
                {
                    // En caso de error, la transacción se revertirá automáticamente
                    throw;
                }
            }
        }
        
        #endregion
    }
} 