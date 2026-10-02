using ECommerce.Domin.Contracts;
using ECommerce.Domin.Entitie;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ECommerce.Persistence
{

    public class SpesificationEvalutor
    {
        public static IQueryable<TEntity> CreateQuery<TEntity, TKey>(
            IQueryable<TEntity> entryPoint,
            ISpesification<TEntity, TKey> specification,
            bool applyPaging = true)
            where TEntity : BaseEntity<TKey>
        {
            var query = entryPoint;

            if (specification is null)
                return query;

            if (specification.Criteria is not null)
                query = query.Where(specification.Criteria);

            if (specification.IncludeExpression is not null &&
                specification.IncludeExpression.Any())
            {
                query = specification.IncludeExpression.Aggregate(
                    query,
                    (currentQuery, includeExp) => currentQuery.Include(includeExp));
            }

            if (specification.OrderBy is not null)
                query = query.OrderBy(specification.OrderBy);

            if (specification.OrderByDescending is not null)
                query = query.OrderByDescending(specification.OrderByDescending);

            if (applyPaging)
            {
                if (specification.Skip.HasValue)
                    query = query.Skip(specification.Skip.Value);

                if (specification.Take.HasValue)
                    query = query.Take(specification.Take.Value);
            }

            return query;
        }
    }
}
