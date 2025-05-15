using System.ComponentModel.DataAnnotations;
using AppMultiTenant.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppMultiTenant.Server.Controllers
{
    /// <summary>
    /// Controlador para la gestión de definiciones de secciones dentro de un inquilino por el Administrador de Inquilino (AI)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    // TODO: Agregar autorización específica para Administrador de Inquilino cuando se implemente
    public class TenantSectionDefinitionsController : ControllerBase
    {
        private readonly ITenantSectionDefinitionService _sectionDefinitionService;
        private readonly IValidationService _validationService;

        /// <summary>
        /// Constructor del controlador de gestión de definiciones de sección del inquilino
        /// </summary>
        /// <param name="sectionDefinitionService">Servicio de gestión de definiciones de sección</param>
        /// <param name="validationService">Servicio de validación</param>
        public TenantSectionDefinitionsController(
            ITenantSectionDefinitionService sectionDefinitionService,
            IValidationService validationService)
        {
            _sectionDefinitionService = sectionDefinitionService ?? throw new ArgumentNullException(nameof(sectionDefinitionService));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        }

        /// <summary>
        /// Obtiene todas las definiciones de sección del inquilino actual
        /// </summary>
        /// <returns>Lista de definiciones de sección</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllSectionDefinitions()
        {
            var sectionDefinitions = await _sectionDefinitionService.GetAllSectionDefinitionsAsync();
            return Ok(sectionDefinitions);
        }

        /// <summary>
        /// Obtiene una definición de sección por su ID
        /// </summary>
        /// <param name="id">ID de la definición de sección</param>
        /// <returns>Información de la definición de sección solicitada</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSectionDefinitionById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID de la definición de sección no puede estar vacío." });
            }

            var sectionDefinition = await _sectionDefinitionService.GetSectionDefinitionByIdAsync(id);

            if (sectionDefinition == null)
            {
                return NotFound(new { message = $"No se encontró ninguna definición de sección con el ID {id}." });
            }

            return Ok(sectionDefinition);
        }

        /// <summary>
        /// Obtiene una definición de sección por su nombre
        /// </summary>
        /// <param name="name">Nombre de la definición de sección</param>
        /// <returns>Información de la definición de sección solicitada</returns>
        [HttpGet("by-name")]
        public async Task<IActionResult> GetSectionDefinitionByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { message = "El nombre de la definición de sección no puede estar vacío." });
            }

            var sectionDefinition = await _sectionDefinitionService.GetSectionDefinitionByNameAsync(name);

            if (sectionDefinition == null)
            {
                return NotFound(new { message = $"No se encontró ninguna definición de sección con el nombre '{name}'." });
            }

            return Ok(sectionDefinition);
        }

        /// <summary>
        /// Verifica si un nombre de definición de sección está disponible dentro del inquilino
        /// </summary>
        /// <param name="name">Nombre a verificar</param>
        /// <param name="excludeSectionDefinitionId">ID de definición de sección a excluir de la verificación (opcional)</param>
        /// <returns>True si está disponible, False si ya existe</returns>
        [HttpGet("check-name")]
        public async Task<IActionResult> CheckSectionDefinitionNameAvailability(
            [FromQuery] string name,
            [FromQuery] Guid? excludeSectionDefinitionId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { message = "El nombre de la definición de sección no puede estar vacío." });
            }

            bool isAvailable = await _sectionDefinitionService.IsSectionNameAvailableAsync(name, excludeSectionDefinitionId);

            return Ok(new { isAvailable });
        }

        /// <summary>
        /// Crea una nueva definición de sección en el inquilino actual
        /// </summary>
        /// <param name="model">Datos de la nueva definición de sección</param>
        /// <returns>Definición de sección creada</returns>
        [HttpPost]
        public async Task<IActionResult> CreateSectionDefinition([FromBody] CreateSectionDefinitionRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var sectionDefinition = await _sectionDefinitionService.CreateSectionDefinitionAsync(
                    model.Name,
                    model.Description);

                // Si se especificó un rol de administrador, crear y asignar permisos para esta sección
                if (model.AdminRoleId.HasValue && model.AdminRoleId != Guid.Empty)
                {
                    await _sectionDefinitionService.CreateAndAssignSectionPermissionsAsync(
                        sectionDefinition.Id,
                        sectionDefinition.Name,
                        model.AdminRoleId.Value);
                }

                return CreatedAtAction(nameof(GetSectionDefinitionById), new { id = sectionDefinition.Id }, sectionDefinition);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza el nombre de una definición de sección existente
        /// </summary>
        /// <param name="id">ID de la definición de sección</param>
        /// <param name="model">Datos actualizados</param>
        /// <returns>Definición de sección actualizada</returns>
        [HttpPut("{id}/name")]
        public async Task<IActionResult> UpdateSectionDefinitionName(Guid id, [FromBody] UpdateSectionDefinitionNameRequest model)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID de la definición de sección no puede estar vacío." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var sectionDefinition = await _sectionDefinitionService.UpdateSectionDefinitionNameAsync(id, model.Name);

                if (sectionDefinition == null)
                {
                    return NotFound(new { message = $"No se encontró ninguna definición de sección con el ID {id}." });
                }

                return Ok(sectionDefinition);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza la descripción de una definición de sección existente
        /// </summary>
        /// <param name="id">ID de la definición de sección</param>
        /// <param name="model">Datos actualizados</param>
        /// <returns>Definición de sección actualizada</returns>
        [HttpPut("{id}/description")]
        public async Task<IActionResult> UpdateSectionDefinitionDescription(Guid id, [FromBody] UpdateSectionDefinitionDescriptionRequest model)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID de la definición de sección no puede estar vacío." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var sectionDefinition = await _sectionDefinitionService.UpdateSectionDefinitionDescriptionAsync(id, model.Description);

                if (sectionDefinition == null)
                {
                    return NotFound(new { message = $"No se encontró ninguna definición de sección con el ID {id}." });
                }

                return Ok(sectionDefinition);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una definición de sección existente
        /// </summary>
        /// <param name="id">ID de la definición de sección a eliminar</param>
        /// <param name="force">Indica si se debe forzar la eliminación incluso si hay datos asociados</param>
        /// <returns>True si se eliminó correctamente, False si no se pudo eliminar</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSectionDefinition(Guid id, [FromQuery] bool force = false)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID de la definición de sección no puede estar vacío." });
            }

            try
            {
                // Verificar primero si la definición de sección puede ser eliminada
                if (!force)
                {
                    var canDelete = await _sectionDefinitionService.CanDeleteSectionDefinitionAsync(id);
                    if (!canDelete)
                    {
                        return BadRequest(new
                        {
                            message = "La definición de sección no puede ser eliminada porque contiene datos. Use force=true para confirmar la eliminación.",
                            requiresForce = true
                        });
                    }
                }

                var result = await _sectionDefinitionService.DeleteSectionDefinitionAsync(id, force);

                if (!result)
                {
                    return NotFound(new { message = $"No se encontró ninguna definición de sección con el ID {id}." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Crea y asigna permisos para una sección a un rol de administrador
        /// </summary>
        /// <param name="id">ID de la definición de sección</param>
        /// <param name="model">Datos del rol de administrador</param>
        /// <returns>Lista de permisos creados y asignados</returns>
        [HttpPost("{id}/permissions")]
        public async Task<IActionResult> CreateAndAssignSectionPermissions(Guid id, [FromBody] AssignSectionPermissionsRequest model)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID de la definición de sección no puede estar vacío." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Primero verificar que la sección existe
                var sectionDefinition = await _sectionDefinitionService.GetSectionDefinitionByIdAsync(id);
                if (sectionDefinition == null)
                {
                    return NotFound(new { message = $"No se encontró ninguna definición de sección con el ID {id}." });
                }

                var permissions = await _sectionDefinitionService.CreateAndAssignSectionPermissionsAsync(
                    id,
                    sectionDefinition.Name,
                    model.AdminRoleId);

                return Ok(permissions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #region Clases de Solicitudes (Request)

        /// <summary>
        /// Modelo para la creación de una nueva definición de sección
        /// </summary>
        public class CreateSectionDefinitionRequest
        {
            /// <summary>
            /// Nombre de la definición de sección
            /// </summary>
            [Required(ErrorMessage = "El nombre de la definición de sección es requerido.")]
            [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
            public string Name { get; set; }

            /// <summary>
            /// Descripción opcional de la definición de sección
            /// </summary>
            [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
            public string Description { get; set; }

            /// <summary>
            /// ID opcional del rol de administrador al que se asignarán automáticamente permisos para esta sección
            /// </summary>
            public Guid? AdminRoleId { get; set; }
        }

        /// <summary>
        /// Modelo para actualizar el nombre de una definición de sección
        /// </summary>
        public class UpdateSectionDefinitionNameRequest
        {
            /// <summary>
            /// Nuevo nombre para la definición de sección
            /// </summary>
            [Required(ErrorMessage = "El nombre de la definición de sección es requerido.")]
            [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
            public string Name { get; set; }
        }

        /// <summary>
        /// Modelo para actualizar la descripción de una definición de sección
        /// </summary>
        public class UpdateSectionDefinitionDescriptionRequest
        {
            /// <summary>
            /// Nueva descripción para la definición de sección
            /// </summary>
            [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
            public string Description { get; set; }
        }

        /// <summary>
        /// Modelo para asignar permisos de sección a un rol de administrador
        /// </summary>
        public class AssignSectionPermissionsRequest
        {
            /// <summary>
            /// ID del rol de administrador al que se asignarán los permisos
            /// </summary>
            [Required(ErrorMessage = "El ID del rol de administrador es requerido.")]
            public Guid AdminRoleId { get; set; }
        }

        #endregion
    }
}