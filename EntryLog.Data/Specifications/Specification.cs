using System.Linq.Expressions;

namespace EntryLog.Data.Specifications;

public abstract class Specification<TEntity> : ISpecification<TEntity> where TEntity : class
{
    public Expression<Func<TEntity, bool>> Expression { get; private set; } = _ => true;
    public Expression<Func<TEntity, object>>? OrderBy { get; private set; }
    public Expression<Func<TEntity, object>>? OrderByDescending { get; private set; }
    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    public void AndAlso(Expression<Func<TEntity, bool>> expression)
    {
        Expression = SpecificationMethods<TEntity>.And(expression, Expression);
    }

    public void AndOrderBy(Expression<Func<TEntity, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
        OrderByDescending = null;
    }

    public void AndOrderByDescending(Expression<Func<TEntity, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
        OrderBy = null;
    }

    public void ApplyPaging(int take, int skip)
    {
        if (take < 0)
            throw new ArgumentOutOfRangeException(nameof(take));

        if (skip < 0)
            throw new ArgumentOutOfRangeException(nameof(skip));

        Take = take;
        Skip = skip;
        IsPagingEnabled = true;
    }
}
