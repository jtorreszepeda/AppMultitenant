using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppMultiTenant.Application.Interfaces.Persistence;
using AppMultiTenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppMultiTenant.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementación del repositorio para la entidad Tenant
    /// </summary>
    public class TenantRepository : RepositoryBase<Tenant>, ITenantRepository
    {
        /// <summary>
        /// Constructor del repositorio de Tenant
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public TenantRepository(AppDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<Tenant> GetByIdentifierAsync(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("El identificador no puede estar vacío", nameof(identifier));

            return await _dbSet.FirstOrDefaultAsync(t => t.Identifier == identifier);
        }

        /// <inheritdoc/>
        public async Task<bool> IsIdentifierAvailableAsync(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("El identificador no puede estar vacío", nameof(identifier));

            // El identificador está disponible si no existe ningún tenant con ese identifier
            return !await _dbSet.AnyAsync(t => t.Identifier == identifier);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Tenant>> GetAllActiveAsync()
        {
            return await _dbSet.Where(t => t.IsActive).ToListAsync();
        }
    }
} 