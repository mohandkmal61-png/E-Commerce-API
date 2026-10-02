using ECommerce.Domin.Contracts;
using ECommerce.Domin.Entitie;
using ECommerce.Persistence.Data.Dbcontexts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Persistence.Repositoris
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StoreDbcontext _dbContext;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(StoreDbcontext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<int> SaveChangesAsync()
        {
            return _dbContext.SaveChangesAsync();
        }

        public IGenarecReprository<TEntity, TKey> GetRepository<TEntity, TKey>()
            where TEntity : BaseEntity<TKey>
        {
            var entityType = typeof(TEntity);

            if (_repositories.TryGetValue(entityType, out var repository))
            {
                return (IGenarecReprository<TEntity, TKey>)repository;
            }

            var newRepository = new GenericRepository<TEntity, TKey>(_dbContext);
            _repositories[entityType] = newRepository;

            return newRepository;
        }
    }
}
