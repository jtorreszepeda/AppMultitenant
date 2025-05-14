using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppMultiTenant.Application.Interfaces.Services;
using FluentValidation;
using FluentValidation.Results;

namespace AppMultiTenant.Application.Services
{
    /// <summary>
    /// Implementación del servicio de validación que utiliza FluentValidation
    /// </summary>
    public class ValidationService : IValidationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDictionary<Type, Type> _validatorTypes;

        /// <summary>
        /// Constructor del servicio de validación
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para resolver validadores</param>
        public ValidationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _validatorTypes = new Dictionary<Type, Type>();
        }

        /// <inheritdoc />
        public async Task<ValidationResult> ValidateAsync<T>(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var validator = GetValidator<T>();
            if (validator == null)
                return new ValidationResult();

            return await validator.ValidateAsync(instance);
        }

        /// <inheritdoc />
        public async Task ValidateAndThrowAsync<T>(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var validator = GetValidator<T>();
            if (validator == null)
                return;

            await validator.ValidateAndThrowAsync(instance);
        }

        /// <summary>
        /// Obtiene un validador para un tipo específico
        /// </summary>
        /// <typeparam name="T">Tipo a validar</typeparam>
        /// <returns>Validador para el tipo o null si no hay validador registrado</returns>
        private IValidator<T> GetValidator<T>()
        {
            Type validatorType = typeof(IValidator<T>);
            
            try
            {
                return (IValidator<T>)_serviceProvider.GetService(validatorType);
            }
            catch
            {
                // Si no se puede resolver el validador, retornamos null
                return null;
            }
        }
    }

    /// <summary>
    /// Extensión de ValidationException para mostrar mensajes de error
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Obtiene un mensaje de error formateado a partir de una ValidationResult
        /// </summary>
        /// <param name="validationResult">Resultado de validación</param>
        /// <returns>Mensaje de error formateado</returns>
        public static string GetErrorMessage(this ValidationResult validationResult)
        {
            if (validationResult.IsValid)
                return string.Empty;

            var errorMessages = validationResult.Errors
                .Select(error => $"• {error.ErrorMessage}")
                .ToList();

            return string.Join(Environment.NewLine, errorMessages);
        }
    }
} 