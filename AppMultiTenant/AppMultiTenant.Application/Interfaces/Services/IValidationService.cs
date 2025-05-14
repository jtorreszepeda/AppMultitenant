using System.Threading.Tasks;
using FluentValidation.Results;

namespace AppMultiTenant.Application.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de validación de datos
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Valida un objeto usando el validador apropiado
        /// </summary>
        /// <typeparam name="T">Tipo de objeto a validar</typeparam>
        /// <param name="instance">Instancia del objeto a validar</param>
        /// <returns>Resultado de la validación</returns>
        Task<ValidationResult> ValidateAsync<T>(T instance);
        
        /// <summary>
        /// Valida un objeto y lanza una excepción si la validación falla
        /// </summary>
        /// <typeparam name="T">Tipo de objeto a validar</typeparam>
        /// <param name="instance">Instancia del objeto a validar</param>
        /// <exception cref="ValidationException">Si la validación falla</exception>
        Task ValidateAndThrowAsync<T>(T instance);
    }
} 