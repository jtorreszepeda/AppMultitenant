using System.ComponentModel.DataAnnotations;
using AppMultiTenant.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AppMultiTenant.Server.Controllers
{
    /// <summary>
    /// Controlador para la gestión de roles y asignación de permisos dentro de un inquilino por el Administrador de Inquilino (AI)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireTenantAccess")]
    public class TenantRolesController : ControllerBase
    {
        private readonly ITenantRoleService _roleService;
        private readonly IValidationService _validationService;

        /// <summary>
        /// Constructor del controlador de gestión de roles del inquilino
        /// </summary>
        /// <param name="roleService">Servicio de gestión de roles</param>
        /// <param name="validationService">Servicio de validación</param>
        public TenantRolesController(
            ITenantRoleService roleService,
            IValidationService validationService)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        }

        /// <summary>
        /// Obtiene todos los roles del inquilino actual
        /// </summary>
        /// <returns>Lista de roles</returns>
        [HttpGet]
        [Authorize(Policy = "ViewRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        /// <summary>
        /// Obtiene un rol por su ID
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <returns>Información del rol solicitado</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "ViewRoles")]
        public async Task<IActionResult> GetRoleById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del rol no puede estar vacío." });
            }

            var role = await _roleService.GetRoleByIdAsync(id);

            if (role == null)
            {
                return NotFound(new { message = $"No se encontró ningún rol con el ID {id}." });
            }

            return Ok(role);
        }

        /// <summary>
        /// Obtiene un rol por su nombre
        /// </summary>
        /// <param name="name">Nombre del rol</param>
        /// <returns>Información del rol solicitado</returns>
        [HttpGet("by-name")]
        [Authorize(Policy = "ViewRoles")]
        public async Task<IActionResult> GetRoleByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { message = "El nombre del rol no puede estar vacío." });
            }

            var role = await _roleService.GetRoleByNameAsync(name);

            if (role == null)
            {
                return NotFound(new { message = $"No se encontró ningún rol con el nombre '{name}'." });
            }

            return Ok(role);
        }

        /// <summary>
        /// Verifica si un nombre de rol está disponible dentro del inquilino
        /// </summary>
        /// <param name="name">Nombre a verificar</param>
        /// <param name="excludeRoleId">ID de rol a excluir de la verificación (opcional)</param>
        /// <returns>True si está disponible, False si ya existe</returns>
        [HttpGet("check-name")]
        [Authorize(Policy = "ViewRoles")]
        public async Task<IActionResult> CheckRoleNameAvailability(
            [FromQuery] string name,
            [FromQuery] Guid? excludeRoleId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { message = "El nombre del rol no puede estar vacío." });
            }

            bool isAvailable = await _roleService.IsRoleNameAvailableAsync(name, excludeRoleId);

            return Ok(new { isAvailable });
        }

        /// <summary>
        /// Crea un nuevo rol en el inquilino actual
        /// </summary>
        /// <param name="model">Datos del nuevo rol</param>
        /// <returns>Rol creado</returns>
        [HttpPost]
        [Authorize(Policy = "CreateRole")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = await _roleService.CreateRoleAsync(
                model.Name,
                model.Description);

            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
        }

        /// <summary>
        /// Actualiza el nombre de un rol existente
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <param name="model">Datos actualizados</param>
        /// <returns>Rol actualizado</returns>
        [HttpPut("{id}/name")]
        [Authorize(Policy = "EditRole")]
        public async Task<IActionResult> UpdateRoleName(Guid id, [FromBody] UpdateRoleNameRequest model)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del rol no puede estar vacío." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = await _roleService.UpdateRoleNameAsync(id, model.Name);

            if (role == null)
            {
                return NotFound(new { message = $"No se encontró ningún rol con el ID {id}." });
            }

            return Ok(role);
        }

        /// <summary>
        /// Actualiza la descripción de un rol existente
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <param name="model">Datos actualizados</param>
        /// <returns>Rol actualizado</returns>
        [HttpPut("{id}/description")]
        [Authorize(Policy = "EditRole")]
        public async Task<IActionResult> UpdateRoleDescription(Guid id, [FromBody] UpdateRoleDescriptionRequest model)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del rol no puede estar vacío." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = await _roleService.UpdateRoleDescriptionAsync(id, model.Description);

            if (role == null)
            {
                return NotFound(new { message = $"No se encontró ningún rol con el ID {id}." });
            }

            return Ok(role);
        }

        /// <summary>
        /// Elimina un rol existente
        /// </summary>
        /// <param name="id">ID del rol a eliminar</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "DeleteRole")]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del rol no puede estar vacío." });
            }

            var success = await _roleService.DeleteRoleAsync(id);

            if (!success)
            {
                return BadRequest(new { message = $"No se pudo eliminar el rol con ID {id}. Puede estar en uso por usuarios." });
            }

            return NoContent();
        }

        /// <summary>
        /// Obtiene todos los permisos disponibles en el sistema
        /// </summary>
        /// <returns>Lista de permisos</returns>
        [HttpGet("all-permissions")]
        [Authorize(Policy = "ViewRoles")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _roleService.GetAllPermissionsAsync();
            return Ok(permissions);
        }

        /// <summary>
        /// Obtiene todos los permisos asignados a un rol
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <returns>Lista de permisos asignados al rol</returns>
        [HttpGet("{id}/permissions")]
        [Authorize(Policy = "ViewRoles")]
        public async Task<IActionResult> GetRolePermissions(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del rol no puede estar vacío." });
            }

            var permissions = await _roleService.GetRolePermissionsAsync(id);

            if (permissions == null)
            {
                return NotFound(new { message = $"No se encontró ningún rol con el ID {id}." });
            }

            return Ok(permissions);
        }

        /// <summary>
        /// Asigna permisos a un rol existente
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <param name="model">Lista de IDs de permisos a asignar</param>
        /// <returns>Lista actualizada de permisos asignados al rol</returns>
        [HttpPost("{id}/permissions")]
        [Authorize(Policy = "AssignPermissions")]
        public async Task<IActionResult> AssignPermissionsToRole(Guid id, [FromBody] AssignPermissionsRequest model)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del rol no puede estar vacío." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedPermissions = await _roleService.AssignPermissionsToRoleAsync(id, model.PermissionIds);

            if (updatedPermissions == null)
            {
                return NotFound(new { message = $"No se encontró ningún rol con el ID {id}." });
            }

            return Ok(updatedPermissions);
        }

        /// <summary>
        /// Elimina permisos de un rol existente
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <param name="model">Lista de IDs de permisos a remover</param>
        /// <returns>Lista actualizada de permisos asignados al rol</returns>
        [HttpDelete("{id}/permissions")]
        [Authorize(Policy = "AssignPermissions")]
        public async Task<IActionResult> RemovePermissionsFromRole(Guid id, [FromBody] RemovePermissionsRequest model)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del rol no puede estar vacío." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedPermissions = await _roleService.RemovePermissionsFromRoleAsync(id, model.PermissionIds);

            if (updatedPermissions == null)
            {
                return NotFound(new { message = $"No se encontró ningún rol con el ID {id}." });
            }

            return Ok(updatedPermissions);
        }

        #region Request Models

        /// <summary>
        /// Modelo para la creación de un nuevo rol
        /// </summary>
        public class CreateRoleRequest
        {
            /// <summary>
            /// Nombre del rol (único dentro del inquilino)
            /// </summary>
            [Required(ErrorMessage = "El nombre del rol es requerido.")]
            [StringLength(100, ErrorMessage = "El nombre del rol no puede exceder los 100 caracteres.")]
            public string Name { get; set; }

            /// <summary>
            /// Descripción del rol (opcional)
            /// </summary>
            [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
            public string Description { get; set; }
        }

        /// <summary>
        /// Modelo para actualizar el nombre de un rol
        /// </summary>
        public class UpdateRoleNameRequest
        {
            /// <summary>
            /// Nuevo nombre del rol
            /// </summary>
            [Required(ErrorMessage = "El nombre del rol es requerido.")]
            [StringLength(100, ErrorMessage = "El nombre del rol no puede exceder los 100 caracteres.")]
            public string Name { get; set; }
        }

        /// <summary>
        /// Modelo para actualizar la descripción de un rol
        /// </summary>
        public class UpdateRoleDescriptionRequest
        {
            /// <summary>
            /// Nueva descripción del rol
            /// </summary>
            [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
            public string Description { get; set; }
        }

        /// <summary>
        /// Modelo para asignar permisos a un rol
        /// </summary>
        public class AssignPermissionsRequest
        {
            /// <summary>
            /// Lista de IDs de permisos a asignar al rol
            /// </summary>
            [Required(ErrorMessage = "Se requiere al menos un permiso para asignar.")]
            public IEnumerable<Guid> PermissionIds { get; set; }
        }

        /// <summary>
        /// Modelo para remover permisos de un rol
        /// </summary>
        public class RemovePermissionsRequest
        {
            /// <summary>
            /// Lista de IDs de permisos a remover del rol
            /// </summary>
            [Required(ErrorMessage = "Se requiere al menos un permiso para remover.")]
            public IEnumerable<Guid> PermissionIds { get; set; }
        }

        #endregion
    }
}