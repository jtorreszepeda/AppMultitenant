namespace AppMultiTenant.Server.Documentation
{
    /// <summary>
    /// Documentación sobre la implementación de validación de modelos en los controladores
    /// </summary>
    public static class ModelValidationDocumentation
    {
        /* 
         * Enfoque de Validación de Modelos en la API
         * =========================================
         * 
         * La validación de modelos en nuestra API implementa un enfoque en capas para garantizar
         * la integridad de los datos recibidos desde el cliente:
         * 
         * 1. Validación de DataAnnotations (Básica):
         *    - Se aplica automáticamente a nivel de controlador para las clases de solicitud (Request).
         *    - Utiliza atributos como [Required], [StringLength], [EmailAddress], etc.
         *    - Es la primera capa de validación y actúa como un filtro rápido.
         * 
         * 2. Validación de Entidades de Dominio (Avanzada):
         *    - Implementada mediante el filtro global ModelValidationFilter.
         *    - Utiliza el servicio IValidationService y FluentValidation para validar modelos complejos.
         *    - Se ejecuta automáticamente para todos los parámetros de acción en todos los controladores.
         *    - Permite validaciones más complejas y específicas del dominio.
         * 
         * Flujo de Validación:
         * -------------------
         * 1. El modelo se deserializa desde la solicitud HTTP.
         * 2. ASP.NET Core aplica automáticamente las validaciones de DataAnnotations.
         * 3. Si esas validaciones pasan, ModelValidationFilter valida el modelo usando FluentValidation.
         * 4. Si alguna validación falla, se devuelve un BadRequest (400) con detalles de los errores.
         * 5. Solo si todas las validaciones pasan, se ejecuta el método del controlador.
         * 
         * Beneficios:
         * ----------
         * - Validación consistente en toda la API.
         * - Separación clara entre validaciones simples (DataAnnotations) y reglas de negocio (FluentValidation).
         * - Mayor seguridad al validar las entidades de dominio antes de procesarlas.
         * - Código más limpio en los controladores, evitando validaciones repetitivas.
         * 
         * Ejemplo de Uso en Código de Controlador:
         * ------------------------------------
         * ```csharp
         * [HttpPost]
         * public async Task<IActionResult> CreateEntity([FromBody] CreateEntityRequest model)
         * {
         *     // No es necesario verificar ModelState.IsValid aquí, ya lo hace el filtro automáticamente
         *     
         *     // El modelo ya está validado cuando llegamos aquí
         *     var entity = await _service.CreateEntityAsync(model.Property1, model.Property2);
         *     
         *     return CreatedAtAction(nameof(GetEntityById), new { id = entity.Id }, entity);
         * }
         * ```
         * 
         * Ejemplo de Definición de un Validador de FluentValidation:
         * -----------------------------------------------------
         * ```csharp
         * public class CreateEntityValidator : AbstractValidator<CreateEntityRequest>
         * {
         *     public CreateEntityValidator()
         *     {
         *         RuleFor(x => x.Property1).NotEmpty().MaximumLength(100);
         *         RuleFor(x => x.Property2).GreaterThan(0).WithMessage("El valor debe ser positivo");
         *     }
         * }
         * ```
         */
    }
} 