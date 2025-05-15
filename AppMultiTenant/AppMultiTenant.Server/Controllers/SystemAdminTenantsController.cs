using System.ComponentModel.DataAnnotations;
using AppMultiTenant.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppMultiTenant.Server.Controllers
{
    /// <summary>
    /// Controlador para la gestión de inquilinos (tenants) por el Super Administrador
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    // TODO: Agregar autorización específica para Super Admin cuando se implemente
    public class SystemAdminTenantsController : ControllerBase
    {
        private readonly ISystemAdminTenantService _tenantService;
        private readonly IValidationService _validationService;

        /// <summary>
        /// Constructor del controlador de gestión de inquilinos
        /// </summary>
        /// <param name="tenantService">Servicio de gestión de inquilinos</param>
        /// <param name="validationService">Servicio de validación</param>
        public SystemAdminTenantsController(
            ISystemAdminTenantService tenantService,
            IValidationService validationService)
        {
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        }

        /// <summary>
        /// Obtiene todos los inquilinos con paginación
        /// </summary>
        /// <param name="includeInactive">Indica si se deben incluir inquilinos inactivos</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Lista paginada de inquilinos</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllTenants(
            [FromQuery] bool includeInactive = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(new { message = "Los parámetros de paginación deben ser mayores a 0." });
            }

            var (tenants, totalCount) = await _tenantService.GetAllTenantsAsync(includeInactive, pageNumber, pageSize);

            return Ok(new
            {
                tenants,
                totalCount,
                pageNumber,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        /// <summary>
        /// Obtiene un inquilino por su ID
        /// </summary>
        /// <param name="id">ID del inquilino</param>
        /// <returns>Información del inquilino solicitado</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTenantById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del inquilino no puede estar vacío." });
            }

            var tenant = await _tenantService.GetTenantByIdAsync(id);

            if (tenant == null)
            {
                return NotFound(new { message = $"No se encontró ningún inquilino con el ID {id}." });
            }

            return Ok(tenant);
        }

        /// <summary>
        /// Obtiene un inquilino por su identificador (ej. subdominio)
        /// </summary>
        /// <param name="identifier">Identificador único del inquilino</param>
        /// <returns>Información del inquilino solicitado</returns>
        [HttpGet("by-identifier/{identifier}")]
        public async Task<IActionResult> GetTenantByIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return BadRequest(new { message = "El identificador no puede estar vacío." });
            }

            var tenant = await _tenantService.GetTenantByIdentifierAsync(identifier);

            if (tenant == null)
            {
                return NotFound(new { message = $"No se encontró ningún inquilino con el identificador '{identifier}'." });
            }

            return Ok(tenant);
        }

        /// <summary>
        /// Verifica si un identificador de inquilino está disponible
        /// </summary>
        /// <param name="identifier">Identificador a verificar</param>
        /// <returns>True si está disponible, False si ya existe</returns>
        [HttpGet("check-identifier/{identifier}")]
        public async Task<IActionResult> CheckIdentifierAvailability(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return BadRequest(new { message = "El identificador no puede estar vacío." });
            }

            bool isAvailable = await _tenantService.IsTenantIdentifierAvailableAsync(identifier);

            return Ok(new { isAvailable });
        }

        /// <summary>
        /// Crea un nuevo inquilino
        /// </summary>
        /// <param name="model">Datos del nuevo inquilino</param>
        /// <returns>Inquilino creado</returns>
        [HttpPost]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var tenant = await _tenantService.CreateTenantAsync(model.Name, model.Identifier);

                // Si se proporcionaron datos del administrador inicial, crearlo
                if (!string.IsNullOrWhiteSpace(model.AdminEmail) && !string.IsNullOrWhiteSpace(model.AdminPassword))
                {
                    await _tenantService.CreateInitialTenantAdminAsync(
                        tenant.TenantId,
                        model.AdminEmail,
                        model.AdminUserName ?? model.AdminEmail,
                        model.AdminPassword,
                        model.AdminFullName ?? model.AdminEmail
                    );
                }

                return CreatedAtAction(nameof(GetTenantById), new { id = tenant.TenantId }, tenant);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza el nombre de un inquilino existente
        /// </summary>
        /// <param name="id">ID del inquilino</param>
        /// <param name="model">Datos actualizados</param>
        /// <returns>Inquilino actualizado</returns>
        [HttpPut("{id}/name")]
        public async Task<IActionResult> UpdateTenantName(Guid id, [FromBody] UpdateTenantNameRequest model)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del inquilino no puede estar vacío." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var tenant = await _tenantService.UpdateTenantNameAsync(id, model.Name);

                if (tenant == null)
                {
                    return NotFound(new { message = $"No se encontró ningún inquilino con el ID {id}." });
                }

                return Ok(tenant);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza el identificador de un inquilino existente
        /// </summary>
        /// <param name="id">ID del inquilino</param>
        /// <param name="model">Datos actualizados</param>
        /// <returns>Inquilino actualizado</returns>
        [HttpPut("{id}/identifier")]
        public async Task<IActionResult> UpdateTenantIdentifier(Guid id, [FromBody] UpdateTenantIdentifierRequest model)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del inquilino no puede estar vacío." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var tenant = await _tenantService.UpdateTenantIdentifierAsync(id, model.Identifier);

                if (tenant == null)
                {
                    return NotFound(new { message = $"No se encontró ningún inquilino con el ID {id}." });
                }

                return Ok(tenant);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Activa un inquilino
        /// </summary>
        /// <param name="id">ID del inquilino</param>
        /// <returns>Inquilino actualizado</returns>
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateTenant(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del inquilino no puede estar vacío." });
            }

            try
            {
                var tenant = await _tenantService.ActivateTenantAsync(id);

                if (tenant == null)
                {
                    return NotFound(new { message = $"No se encontró ningún inquilino con el ID {id}." });
                }

                return Ok(tenant);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Desactiva un inquilino
        /// </summary>
        /// <param name="id">ID del inquilino</param>
        /// <returns>Inquilino actualizado</returns>
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateTenant(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del inquilino no puede estar vacío." });
            }

            try
            {
                var tenant = await _tenantService.DeactivateTenantAsync(id);

                if (tenant == null)
                {
                    return NotFound(new { message = $"No se encontró ningún inquilino con el ID {id}." });
                }

                return Ok(tenant);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un inquilino completamente (operación peligrosa)
        /// </summary>
        /// <param name="id">ID del inquilino</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTenant(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del inquilino no puede estar vacío." });
            }

            try
            {
                bool deleted = await _tenantService.DeleteTenantAsync(id);

                if (!deleted)
                {
                    return NotFound(new { message = $"No se encontró ningún inquilino con el ID {id} o no se pudo eliminar." });
                }

                return Ok(new { message = $"Inquilino con ID {id} eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Modelo de solicitud para crear un nuevo inquilino
    /// </summary>
    public class CreateTenantRequest
    {
        /// <summary>
        /// Nombre del inquilino
        /// </summary>
        [Required(ErrorMessage = "El nombre del inquilino es obligatorio.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        public string Name { get; set; }

        /// <summary>
        /// Identificador único para acceso (subdominio, etc.)
        /// </summary>
        [Required(ErrorMessage = "El identificador del inquilino es obligatorio.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El identificador debe tener entre 2 y 50 caracteres.")]
        [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "El identificador solo puede contener letras minúsculas, números y guiones.")]
        public string Identifier { get; set; }

        /// <summary>
        /// Email del administrador inicial (opcional)
        /// </summary>
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string AdminEmail { get; set; }

        /// <summary>
        /// Nombre de usuario del administrador inicial (opcional)
        /// </summary>
        public string AdminUserName { get; set; }

        /// <summary>
        /// Contraseña del administrador inicial (opcional)
        /// </summary>
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string AdminPassword { get; set; }

        /// <summary>
        /// Nombre completo del administrador inicial (opcional)
        /// </summary>
        public string AdminFullName { get; set; }
    }

    /// <summary>
    /// Modelo de solicitud para actualizar el nombre de un inquilino
    /// </summary>
    public class UpdateTenantNameRequest
    {
        /// <summary>
        /// Nuevo nombre del inquilino
        /// </summary>
        [Required(ErrorMessage = "El nombre del inquilino es obligatorio.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        public string Name { get; set; }
    }

    /// <summary>
    /// Modelo de solicitud para actualizar el identificador de un inquilino
    /// </summary>
    public class UpdateTenantIdentifierRequest
    {
        /// <summary>
        /// Nuevo identificador del inquilino
        /// </summary>
        [Required(ErrorMessage = "El identificador del inquilino es obligatorio.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El identificador debe tener entre 2 y 50 caracteres.")]
        [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "El identificador solo puede contener letras minúsculas, números y guiones.")]
        public string Identifier { get; set; }
    }
}