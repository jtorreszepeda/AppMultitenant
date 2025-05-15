namespace AppMultiTenant.Domain.Exceptions
{
    /// <summary>
    /// Excepción base para todas las excepciones de dominio
    /// </summary>
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message)
        {
        }

        protected DomainException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Excepción para cuando una entidad no se encuentra
    /// </summary>
    public class NotFoundException : DomainException
    {
        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string entityName, object key)
            : base($"La entidad {entityName} con identificador {key} no fue encontrada.")
        {
        }
    }

    /// <summary>
    /// Excepción para cuando una operación no está autorizada por el usuario actual
    /// </summary>
    public class AccessDeniedException : DomainException
    {
        public AccessDeniedException(string message) : base(message)
        {
        }

        public AccessDeniedException()
            : base("No tienes permiso para acceder a este recurso.")
        {
        }
    }

    /// <summary>
    /// Excepción para cuando un usuario intenta acceder a recursos de un inquilino que no le corresponde
    /// </summary>
    public class TenantMismatchException : DomainException
    {
        public TenantMismatchException(string message) : base(message)
        {
        }

        public TenantMismatchException()
            : base("No tienes acceso a los recursos de este inquilino.")
        {
        }
    }

    /// <summary>
    /// Excepción para validaciones que fallan en el dominio
    /// </summary>
    public class DomainValidationException : DomainException
    {
        public DomainValidationException(string message) : base(message)
        {
        }
    }
}