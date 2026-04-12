namespace QueryForge.Builders;

using System.Linq.Expressions;

internal static class ExpressionBuilderHelpers
{
    public static Expression BuildStringMethod(
        Expression property,
        object? value,
        string methodName)
    {
        if (property.Type != typeof(string))
            throw new ArgumentException($"{methodName} supports only string fields");

        var method = typeof(string).GetMethod(methodName, new[] { typeof(string) })
            ?? throw new InvalidOperationException($"Method '{methodName}' not found");

        var notNull = Expression.NotEqual(
            property,
            Expression.Constant(null, typeof(string)));

        var constant = Expression.Constant(value?.ToString() ?? string.Empty);

        var call = Expression.Call(property, method, constant);

        return Expression.AndAlso(notNull, call);
    }
}
