using ECommerce.Domin.Entitie;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Domin.Contracts
{
    public interface IGenarecReprository<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        Task AddAsync(TEntity entity);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> GetAll(ISpesification<TEntity ,TKey> spesification);
        Task<int> CountAsync(ISpesification<TEntity, TKey> spesification);
        Task<TEntity?> GetByIdAsync(TKey id);
        Task Remove(TEntity entity);
        void Update(TEntity entity);
    }
}
