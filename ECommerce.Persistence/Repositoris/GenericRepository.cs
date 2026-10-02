using ECommerce.Domin.Contracts;
using ECommerce.Domin.Entitie;
using ECommerce.Persistence.Data.Dbcontexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq; 
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Persistence.Repositoris
{
    public class GenericRepository<TEntity, TKey> : IGenarecReprository<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        private readonly StoreDbcontext _dbContext;

        public GenericRepository(StoreDbcontext dbContext) 
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(TEntity entity)
        
           => await _dbContext.Set<TEntity>().AddAsync(entity);
        

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            if (typeof(TEntity) == typeof(ECommerce.Domin.Entitie.ProductModule.Product))
            {
                query = query.Include("ProductBrands")
                             .Include("ProductTypes");
            }
            else if (typeof(TEntity) == typeof(ECommerce.Domin.Entitie.Orders.Order))
            {
                query = query.Include("DeliveryMethod");
            }

            return await query.ToListAsync();
        }

        

        public async Task<TEntity?> GetByIdAsync(TKey id)
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            if (typeof(TEntity) == typeof(ECommerce.Domin.Entitie.ProductModule.Product))
            {
                query = query.Include("ProductBrands")
                             .Include("ProductTypes");
            }
            else if (typeof(TEntity) == typeof(ECommerce.Domin.Entitie.Orders.Order))
            {
                query = query.Include("DeliveryMethod");
            }

            return await query.FirstOrDefaultAsync(entity => entity.Id!.Equals(id));
        }


        public Task Remove(TEntity entity)
        {
            _dbContext.Set<TEntity>().Remove(entity);
            return Task.CompletedTask;
        }
      

        public void Update(TEntity entity)
       =>  _dbContext.Set<TEntity>().Update(entity);


        public async Task<IEnumerable<TEntity>> GetAll(ISpesification<TEntity, TKey> spesification)
        {
            var query = SpesificationEvalutor.CreateQuery(_dbContext.Set<TEntity>(), spesification);
            return await query.ToListAsync();
        }

        public async Task<int> CountAsync(ISpesification<TEntity, TKey> spesification)
        {
            var query = SpesificationEvalutor.CreateQuery(_dbContext.Set<TEntity>(), spesification, applyPaging: false);
            return await query.CountAsync();
        }

    }
}
