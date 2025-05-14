using System;
using System.Threading;
using System.Threading.Tasks;
using AppMultiTenant.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppMultiTenant.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementación del patrón Unit of Work que coordina el trabajo de múltiples repositorios
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction _transaction;
        private bool _disposed;

        // Repositorios
        private ITenantRepository _tenantRepository;
        private IUserRepository _userRepository;
        private IRoleRepository _roleRepository;
        private IPermissionRepository _permissionRepository;
        private IAppSectionDefinitionRepository _appSectionDefinitionRepository;

        /// <summary>
        /// Constructor del UnitOfWork
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public UnitOfWork(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public ITenantRepository Tenants => _tenantRepository ??= new TenantRepository(_context);

        /// <inheritdoc/>
        public IUserRepository Users => _userRepository ??= new UserRepository(_context);

        /// <inheritdoc/>
        public IRoleRepository Roles => _roleRepository ??= new RoleRepository(_context);

        /// <inheritdoc/>
        public IPermissionRepository Permissions => _permissionRepository ??= new PermissionRepository(_context);

        /// <inheritdoc/>
        public IAppSectionDefinitionRepository AppSectionDefinitions => _appSectionDefinitionRepository ??= new AppSectionDefinitionRepository(_context);

        /// <inheritdoc/>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IDisposable> BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
            return _transaction;
        }

        /// <inheritdoc/>
        public async Task CommitTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync().ConfigureAwait(false);
                    _transaction = null;
                }
            }
        }

        /// <inheritdoc/>
        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                }
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync().ConfigureAwait(false);
                    _transaction = null;
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Método para liberación de recursos
        /// </summary>
        /// <param name="disposing">Indica si se están liberando recursos administrados</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
                _transaction?.Dispose();
            }
            _disposed = true;
        }
    }
} 