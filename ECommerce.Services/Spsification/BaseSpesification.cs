using ECommerce.Domin.Contracts;
using ECommerce.Domin.Entitie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Services.Spsification
{
    public class BaseSpesification<TEntity, TKey> : ISpesification<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        public Expression<Func<TEntity, bool>>? Criteria { get; private set; }
        public ICollection<Expression<Func<TEntity, object>>> IncludeExpression { get; } = [];
        public Expression<Func<TEntity, object>>? OrderBy { get; private set; }
        public Expression<Func<TEntity, object>>? OrderByDescending { get; private set; }
        public int? Skip { get; private set; }
        public int? Take { get; private set; }

        protected void AddCriteria(Expression<Func<TEntity, bool>> criteriaExpression)
        {
            Criteria = Criteria is null
                ? criteriaExpression
                : CombineCriteria(Criteria, criteriaExpression);
        }

        protected void addInclude(Expression<Func<TEntity, object>> includeExpression)
        {
            IncludeExpression.Add(includeExpression);
        }

        protected void AddOrderBy(Expression<Func<TEntity, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
            OrderByDescending = null;
        }

        protected void AddOrderByDescending(Expression<Func<TEntity, object>> orderByDescendingExpression)
        {
            OrderByDescending = orderByDescendingExpression;
            OrderBy = null;
        }

        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
        }

        private static Expression<Func<TEntity, bool>> CombineCriteria(
            Expression<Func<TEntity, bool>> left,
            Expression<Func<TEntity, bool>> right)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "entity");
            var leftBody = new ReplaceParameterVisitor(left.Parameters[0], parameter).Visit(left.Body)!;
            var rightBody = new ReplaceParameterVisitor(right.Parameters[0], parameter).Visit(right.Body)!;
            var combined = Expression.AndAlso(leftBody, rightBody);

            return Expression.Lambda<Func<TEntity, bool>>(combined, parameter);
        }

        private sealed class ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
            : ExpressionVisitor
        {
            protected override Expression VisitParameter(ParameterExpression node)
                => node == oldParameter ? newParameter : base.VisitParameter(node);
        }
    }
}
