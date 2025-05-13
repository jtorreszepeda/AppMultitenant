using System;
using System.Threading;
using System.Threading.Tasks;

namespace AppMultiTenant.Application.Interfaces.Persistence
{
    /// <summary>
    /// Interfaz para el patrón Unit of Work, que coordina el trabajo de múltiples repositorios
    /// en una única transacción
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Repositorio de inquilinos
        /// </summary>
        ITenantRepository Tenants { get; }
        
        /// <summary>
        /// Repositorio de usuarios
        /// </summary>
        IUserRepository Users { get; }
        
        /// <summary>
        /// Repositorio de roles
        /// </summary>
        IRoleRepository Roles { get; }
        
        /// <summary>
        /// Repositorio de permisos
        /// </summary>
        IPermissionRepository Permissions { get; }
        
        /// <summary>
        /// Repositorio de definiciones de secciones
        /// </summary>
        IAppSectionDefinitionRepository AppSectionDefinitions { get; }
        
        /// <summary>
        /// Guarda todos los cambios realizados en el contexto a la base de datos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación opcional</param>
        /// <returns>El número de entradas escritas en la base de datos</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Inicia una nueva transacción
        /// </summary>
        /// <returns>Un objeto que representa la transacción iniciada</returns>
        Task<IDisposable> BeginTransactionAsync();
        
        /// <summary>
        /// Confirma la transacción actual
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task CommitTransactionAsync();
        
        /// <summary>
        /// Descarta la transacción actual
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task RollbackTransactionAsync();
    }
} 