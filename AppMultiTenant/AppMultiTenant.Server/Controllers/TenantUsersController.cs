using System.ComponentModel.DataAnnotations;
using AppMultiTenant.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppMultiTenant.Server.Controllers
{
    /// <summary>
    /// Controlador para la gestión de usuarios dentro de un inquilino por el Administrador de Inquilino (AI)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    // TODO: Agregar autorización específica para Administrador de Inquilino cuando se implemente
    public class TenantUsersController : ControllerBase
    {
        private readonly ITenantUserService _userService;
        private readonly IValidationService _validationService;

        /// <summary>
        /// Constructor del controlador de gestión de usuarios del inquilino
        /// </summary>
        /// <param name="userService">Servicio de gestión de usuarios</param>
        /// <param name="validationService">Servicio de validación</param>
        public TenantUsersController(
            ITenantUserService userService,
            IValidationService validationService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        }

        /// <summary>
        /// Obtiene todos los usuarios del inquilino actual con paginación
        /// </summary>
        /// <param name="includeInactive">Indica si se deben incluir usuarios inactivos</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Lista paginada de usuarios</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] bool includeInactive = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(new { message = "Los parámetros de paginación deben ser mayores a 0." });
            }

            var (users, totalCount) = await _userService.GetAllUsersAsync(includeInactive, pageNumber, pageSize);

            return Ok(new
            {
                users,
                totalCount,
                pageNumber,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Información del usuario solicitado</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del usuario no puede estar vacío." });
            }

            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new { message = $"No se encontró ningún usuario con el ID {id}." });
            }

            return Ok(user);
        }

        /// <summary>
        /// Obtiene un usuario por su email
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <returns>Información del usuario solicitado</returns>
        [HttpGet("by-email")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "El email no puede estar vacío." });
            }

            var user = await _userService.GetUserByEmailAsync(email);

            if (user == null)
            {
                return NotFound(new { message = $"No se encontró ningún usuario con el email '{email}'." });
            }

            return Ok(user);
        }

        /// <summary>
        /// Verifica si un email está disponible dentro del inquilino
        /// </summary>
        /// <param name="email">Email a verificar</param>
        /// <param name="excludeUserId">ID de usuario a excluir de la verificación (opcional)</param>
        /// <returns>True si está disponible, False si ya existe</returns>
        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmailAvailability(
            [FromQuery] string email,
            [FromQuery] Guid? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "El email no puede estar vacío." });
            }

            bool isAvailable = await _userService.IsEmailAvailableAsync(email, excludeUserId);

            return Ok(new { isAvailable });
        }

        /// <summary>
        /// Crea un nuevo usuario en el inquilino actual
        /// </summary>
        /// <param name="model">Datos del nuevo usuario</param>
        /// <returns>Usuario creado</returns>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userService.CreateUserAsync(
                    model.UserName,
                    model.Email,
                    model.Password,
                    model.FullName);

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza el nombre completo de un usuario existente
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="model">Datos actualizados</param>
        /// <returns>Usuario actualizado</returns>
        [HttpPut("{id}/fullname")]
        public async Task<IActionResult> UpdateUserFullName(Guid id, [FromBody] UpdateUserFullNameRequest model)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del usuario no puede estar vacío." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userService.UpdateUserFullNameAsync(id, model.FullName);

                if (user == null)
                {
                    return NotFound(new { message = $"No se encontró ningún usuario con el ID {id}." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza el email de un usuario existente
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="model">Datos actualizados</param>
        /// <returns>Usuario actualizado</returns>
        [HttpPut("{id}/email")]
        public async Task<IActionResult> UpdateUserEmail(Guid id, [FromBody] UpdateUserEmailRequest model)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del usuario no puede estar vacío." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userService.UpdateUserEmailAsync(id, model.Email);

                if (user == null)
                {
                    return NotFound(new { message = $"No se encontró ningún usuario con el ID {id}." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza el nombre de usuario de un usuario existente
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="model">Datos actualizados</param>
        /// <returns>Usuario actualizado</returns>
        [HttpPut("{id}/username")]
        public async Task<IActionResult> UpdateUserName(Guid id, [FromBody] UpdateUserNameRequest model)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del usuario no puede estar vacío." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userService.UpdateUserNameAsync(id, model.UserName);

                if (user == null)
                {
                    return NotFound(new { message = $"No se encontró ningún usuario con el ID {id}." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Resetea la contraseña de un usuario (por administrador)
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="model">Nueva contraseña</param>
        /// <returns>Confirmación del cambio</returns>
        [HttpPut("{id}/reset-password")]
        public async Task<IActionResult> ResetUserPassword(Guid id, [FromBody] ResetUserPasswordRequest model)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del usuario no puede estar vacío." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                bool success = await _userService.ResetUserPasswordAsync(id, model.NewPassword);

                if (!success)
                {
                    return NotFound(new { message = $"No se encontró ningún usuario con el ID {id} o no se pudo restablecer la contraseña." });
                }

                return Ok(new { message = "Contraseña restablecida con éxito." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Activa un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Usuario actualizado</returns>
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateUser(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del usuario no puede estar vacío." });
            }

            try
            {
                var user = await _userService.ActivateUserAsync(id);

                if (user == null)
                {
                    return NotFound(new { message = $"No se encontró ningún usuario con el ID {id}." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Desactiva un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Usuario actualizado</returns>
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del usuario no puede estar vacío." });
            }

            try
            {
                var user = await _userService.DeactivateUserAsync(id);

                if (user == null)
                {
                    return NotFound(new { message = $"No se encontró ningún usuario con el ID {id}." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un usuario
        /// </summary>
        /// <param name="id">ID del usuario a eliminar</param>
        /// <param name="currentUserId">ID del usuario que realiza la operación</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id, [FromQuery] Guid currentUserId)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del usuario no puede estar vacío." });
            }

            if (currentUserId == Guid.Empty)
            {
                return BadRequest(new { message = "Se requiere el ID del usuario actual." });
            }

            try
            {
                bool deleted = await _userService.DeleteUserAsync(id, currentUserId);

                if (!deleted)
                {
                    return NotFound(new { message = $"No se encontró ningún usuario con el ID {id} o no se pudo eliminar." });
                }

                return Ok(new { message = $"Usuario con ID {id} eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los roles asignados a un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Lista de roles asignados</returns>
        [HttpGet("{id}/roles")]
        public async Task<IActionResult> GetUserRoles(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del usuario no puede estar vacío." });
            }

            try
            {
                var roles = await _userService.GetUserRolesAsync(id);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Asigna roles a un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="model">Lista de IDs de roles a asignar</param>
        /// <returns>Lista de roles asignados</returns>
        [HttpPost("{id}/roles")]
        public async Task<IActionResult> AssignRolesToUser(Guid id, [FromBody] AssignRolesToUserRequest model)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del usuario no puede estar vacío." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var roles = await _userService.AssignRolesToUserAsync(id, model.RoleIds);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Remueve roles de un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="model">Lista de IDs de roles a remover</param>
        /// <returns>Confirmación de la operación</returns>
        [HttpDelete("{id}/roles")]
        public async Task<IActionResult> RemoveRolesFromUser(Guid id, [FromBody] RemoveRolesFromUserRequest model)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "El ID del usuario no puede estar vacío." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                bool success = await _userService.RemoveRolesFromUserAsync(id, model.RoleIds);

                if (!success)
                {
                    return NotFound(new { message = $"No se encontró ningún usuario con el ID {id} o no se pudieron remover los roles." });
                }

                return Ok(new { message = "Roles removidos correctamente del usuario." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Modelo de solicitud para crear un nuevo usuario
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// Nombre de usuario único
        /// </summary>
        [Required(ErrorMessage = "El nombre de usuario es requerido.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 100 caracteres.")]
        public string UserName { get; set; }

        /// <summary>
        /// Correo electrónico del usuario
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
        public string Email { get; set; }

        /// <summary>
        /// Contraseña inicial del usuario
        /// </summary>
        [Required(ErrorMessage = "La contraseña es requerida.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; }

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        [StringLength(100, ErrorMessage = "El nombre completo no puede exceder los 100 caracteres.")]
        public string FullName { get; set; }
    }

    /// <summary>
    /// Modelo de solicitud para actualizar el nombre completo de un usuario
    /// </summary>
    public class UpdateUserFullNameRequest
    {
        /// <summary>
        /// Nuevo nombre completo del usuario
        /// </summary>
        [Required(ErrorMessage = "El nombre completo es requerido.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre completo debe tener entre 1 y 100 caracteres.")]
        public string FullName { get; set; }
    }

    /// <summary>
    /// Modelo de solicitud para actualizar el email de un usuario
    /// </summary>
    public class UpdateUserEmailRequest
    {
        /// <summary>
        /// Nuevo correo electrónico del usuario
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
        public string Email { get; set; }
    }

    /// <summary>
    /// Modelo de solicitud para actualizar el nombre de usuario
    /// </summary>
    public class UpdateUserNameRequest
    {
        /// <summary>
        /// Nuevo nombre de usuario
        /// </summary>
        [Required(ErrorMessage = "El nombre de usuario es requerido.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 100 caracteres.")]
        public string UserName { get; set; }
    }

    /// <summary>
    /// Modelo de solicitud para resetear la contraseña de un usuario
    /// </summary>
    public class ResetUserPasswordRequest
    {
        /// <summary>
        /// Nueva contraseña del usuario
        /// </summary>
        [Required(ErrorMessage = "La nueva contraseña es requerida.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string NewPassword { get; set; }
    }

    /// <summary>
    /// Modelo de solicitud para asignar roles a un usuario
    /// </summary>
    public class AssignRolesToUserRequest
    {
        /// <summary>
        /// Lista de IDs de roles a asignar
        /// </summary>
        [Required(ErrorMessage = "Se requiere al menos un rol para asignar.")]
        public IEnumerable<Guid> RoleIds { get; set; }
    }

    /// <summary>
    /// Modelo de solicitud para remover roles de un usuario
    /// </summary>
    public class RemoveRolesFromUserRequest
    {
        /// <summary>
        /// Lista de IDs de roles a remover
        /// </summary>
        [Required(ErrorMessage = "Se requiere al menos un rol para remover.")]
        public IEnumerable<Guid> RoleIds { get; set; }
    }
}