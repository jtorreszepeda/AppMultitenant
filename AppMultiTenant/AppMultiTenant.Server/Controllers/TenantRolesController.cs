using System.ComponentModel.DataAnnotations;
using AppMultiTenant.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppMultiTenant.Server.Controllers
{
    /// <summary>
    /// Controlador para la gestión de roles y asignación de permisos dentro de un inquilino por el Administrador de Inquilino (AI)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    // TODO: Agregar autorización específica para Administrador de Inquilino cuando se implemente
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
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var role = await _roleService.CreateRoleAsync(
                    model.Name,
                    model.Description);

                return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza el nombre de un rol existente
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <param name="model">Datos actualizados</param>
        /// <returns>Rol actualizado</returns>
        [HttpPut("{id}/name")]
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

            try
            {
                var role = await _roleService.UpdateRoleNameAsync(id, model.Name);

                if (role == null)
                {
                    return NotFound(new { message = $"No se encontró ningún rol con el ID {id}." });
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza la descripción de un rol existente
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <param name="model">Datos actualizados</param>
        /// <returns>Rol actualizado</returns>
        [HttpPut("{id}/description")]
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

            try
            {
                var role = await _roleService.UpdateRoleDescriptionAsync(id, model.Description);

                if (role == null)
                {
                    return NotFound(new { message = $"No se encontró ningún rol con el ID {id}." });
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un rol existente
        /// </summary>
        /// <param name="id">ID del rol a eliminar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del rol no puede estar vacío." });
            }

            try
            {
                var success = await _roleService.DeleteRoleAsync(id);

                if (!success)
                {
                    return BadRequest(new { message = $"No se pudo eliminar el rol con ID {id}. Puede estar en uso por usuarios." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todos los permisos disponibles en el sistema
        /// </summary>
        /// <returns>Lista de permisos del sistema</returns>
        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _roleService.GetAllPermissionsAsync();
            return Ok(permissions);
        }

        /// <summary>
        /// Obtiene los permisos asignados a un rol específico
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <returns>Lista de permisos asignados al rol</returns>
        [HttpGet("{id}/permissions")]
        public async Task<IActionResult> GetRolePermissions(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del rol no puede estar vacío." });
            }

            // Primero verificamos que el rol exista
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = $"No se encontró ningún rol con el ID {id}." });
            }

            var permissions = await _roleService.GetRolePermissionsAsync(id);
            return Ok(permissions);
        }

        /// <summary>
        /// Asigna permisos a un rol
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <param name="model">Datos de los permisos a asignar</param>
        /// <returns>Lista de permisos asignados</returns>
        [HttpPost("{id}/permissions")]
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

            // Primero verificamos que el rol exista
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = $"No se encontró ningún rol con el ID {id}." });
            }

            try
            {
                var permissions = await _roleService.AssignPermissionsToRoleAsync(id, model.PermissionIds);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Remueve permisos de un rol
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <param name="model">Datos de los permisos a remover</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}/permissions")]
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

            // Primero verificamos que el rol exista
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = $"No se encontró ningún rol con el ID {id}." });
            }

            try
            {
                var success = await _roleService.RemovePermissionsFromRoleAsync(id, model.PermissionIds);

                if (!success)
                {
                    return BadRequest(new { message = "No se pudieron remover algunos de los permisos especificados." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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