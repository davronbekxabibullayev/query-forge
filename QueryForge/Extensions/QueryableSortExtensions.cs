namespace QueryForge.Extensions;

using System.Linq.Expressions;
using QueryForge.Abstractions;

public static class QueryableSortExtensions
{
    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        List<SortRule>? sorts)
    {
        if (sorts == null || sorts.Count == 0)
            return query;

        for (var i = 0; i < sorts.Count; i++)
        {
            var sort = sorts[i];

            var parameter = Expression.Parameter(typeof(T), "x");

            var property = BuildPropertyExpression(parameter, sort.Field);

            var lambda = Expression.Lambda(property, parameter);

            var method = i == 0
                ? (sort.Desc ? "OrderByDescending" : "OrderBy")
                : (sort.Desc ? "ThenByDescending" : "ThenBy");

            var resultExpression = typeof(Queryable)
                .GetMethods()
                .First(m => m.Name == method && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), property.Type)
                .Invoke(null, [query, lambda]);

            query = (IQueryable<T>)resultExpression!;
        }

        return query;
    }

    private static Expression BuildPropertyExpression(Expression parameter, string propertyPath)
    {
        var current = parameter;

        foreach (var prop in propertyPath.Split('.'))
        {
            var propertyInfo = current.Type.GetProperty(prop)
                ?? throw new ArgumentException(
                    $"Property '{prop}' not found on {current.Type.Name}");

            current = Expression.Property(current, propertyInfo);
        }

        return current;
    }
}
