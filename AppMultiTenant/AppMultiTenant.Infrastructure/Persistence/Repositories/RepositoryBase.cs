using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AppMultiTenant.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AppMultiTenant.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementación base para los repositorios, proporciona funcionalidad CRUD genérica
    /// </summary>
    /// <typeparam name="T">Tipo de entidad que gestiona el repositorio</typeparam>
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Constructor del repositorio base
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        protected RepositoryBase(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        /// <inheritdoc/>
        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        /// <inheritdoc/>
        public virtual async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        /// <inheritdoc/>
        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        /// <inheritdoc/>
        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        /// <inheritdoc/>
        public virtual Task UpdateAsync(T entity)
        {
            // EntityFramework rastrea la entidad y la actualizará al guardar cambios
            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task RemoveAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task RemoveRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }
    }
} 