namespace QueryForge.Extensions;

using System.Linq.Expressions;
using QueryForge.Abstractions;
using QueryForge.Builders;

public static class QueryableFilterExtensions
{
    public static IQueryable<T> ApplyFilters<T>(
     this IQueryable<T> query,
     List<FilterRule>? filters)
    {
        if (filters == null || filters.Count == 0)
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");

        Expression? andBody = null;
        Expression? orBody = null;

        foreach (var filter in filters)
        {
            var exp = ExpressionBuilder.Build<T>(parameter, filter);

            if (filter.Logic == FilterLogic.Or)
            {
                orBody = orBody == null ? exp : Expression.OrElse(orBody, exp);
            }
            else
            {
                andBody = andBody == null ? exp : Expression.AndAlso(andBody, exp);
            }
        }

        Expression? body = null;

        if (andBody != null && orBody != null)
            body = Expression.OrElse(andBody, orBody);
        else
            body = andBody ?? orBody;

        var lambda = Expression.Lambda<Func<T, bool>>(body!, parameter);

        return query.Where(lambda);
    }
}
