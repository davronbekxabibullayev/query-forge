namespace QueryForge.Builders;

using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using QueryForge.Abstractions;

public static class ExpressionBuilder
{
    public static Expression Build<T>(
        ParameterExpression parameter,
        FilterRule filter)
    {
        var property = BuildPropertyExpression(parameter, filter.Field);

        var targetType = Nullable.GetUnderlyingType(property.Type) ?? property.Type;

        var constantValue = ConvertValue(filter.Value, targetType);
        var constant = Expression.Constant(constantValue, property.Type);

        return filter.Operator switch
        {
            // ================= BASIC =================
            FilterOperator.Eq => BuildEquals(property, constantValue, targetType),
            FilterOperator.Neq => Expression.Not(BuildEquals(property, constantValue, targetType)),

            FilterOperator.Gt => Expression.GreaterThan(property, constant),
            FilterOperator.Gte => Expression.GreaterThanOrEqual(property, constant),
            FilterOperator.Lt => Expression.LessThan(property, constant),
            FilterOperator.Lte => Expression.LessThanOrEqual(property, constant),

            // ================= STRING =================
            FilterOperator.Contains =>
                ExpressionBuilderHelpers.BuildStringMethod(property, filter.Value, "Contains"),

            FilterOperator.StartsWith =>
                ExpressionBuilderHelpers.BuildStringMethod(property, filter.Value, "StartsWith"),

            FilterOperator.EndsWith =>
                ExpressionBuilderHelpers.BuildStringMethod(property, filter.Value, "EndsWith"),

            FilterOperator.Like => BuildLike(property, filter.Value),
            FilterOperator.NotLike => Expression.Not(BuildLike(property, filter.Value)),

            // ================= COLLECTION =================
            FilterOperator.In => BuildIn(property, filter.Value),
            FilterOperator.NotIn => Expression.Not(BuildIn(property, filter.Value)),

            // ================= RANGE =================
            FilterOperator.Between => BuildBetween(property, filter.Value),
            FilterOperator.NotBetween => Expression.Not(BuildBetween(property, filter.Value)),

            // ================= NULL =================
            FilterOperator.IsNull =>
                Expression.Equal(property, Expression.Constant(null, property.Type)),

            FilterOperator.IsNotNull =>
                Expression.NotEqual(property, Expression.Constant(null, property.Type)),

            FilterOperator.IsEmpty => BuildIsEmpty(property),
            FilterOperator.IsNotEmpty => Expression.Not(BuildIsEmpty(property)),

            FilterOperator.IsNullOrEmpty => BuildIsNullOrEmpty(property),
            FilterOperator.IsNotNullOrEmpty => Expression.Not(BuildIsNullOrEmpty(property)),

            // ================= BOOL =================
            FilterOperator.IsTrue =>
                Expression.Equal(property, Expression.Constant(true)),

            FilterOperator.IsFalse =>
                Expression.Equal(property, Expression.Constant(false)),

            _ => throw new NotSupportedException($"Operator '{filter.Operator}' not supported")
        };
    }

    // ================= EQUALS =================
    private static BinaryExpression BuildEquals(Expression property, object? value, Type targetType)
    {
        if (property.Type == typeof(string))
        {
            var toLower = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;

            var left = Expression.Call(property, toLower);
            var right = Expression.Constant(value?.ToString()?.ToLower());

            return Expression.Equal(left, right);
        }

        return Expression.Equal(
            property,
            Expression.Constant(value, property.Type));
    }

    // ================= LIKE =================
    private static Expression BuildLike(Expression property, object? value)
    {
        if (property.Type != typeof(string))
            throw new ArgumentException("Like supports only string fields");

        var contains = typeof(string).GetMethod("Contains", [typeof(string)])!;
        var toLower = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;

        var left = Expression.Call(property, toLower);
        var right = Expression.Constant(value?.ToString()?.ToLower());

        return Expression.Call(left, contains, right);
    }

    // ================= IN =================
    private static Expression BuildIn(Expression property, object? value)
    {
        if (value is not JsonElement json || json.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("In expects array");

        var list = json.EnumerateArray()
            .Select(x => ConvertValue(x.ToString(), property.Type))
            .ToList();

        var listType = typeof(List<>).MakeGenericType(property.Type);

        var containsMethod = listType.GetMethod("Contains", [property.Type])!;

        var constant = Expression.Constant(list);

        return Expression.Call(constant, containsMethod, property);
    }

    // ================= BETWEEN =================
    private static BinaryExpression BuildBetween(Expression property, object? value)
    {
        if (value is not JsonElement json)
            throw new ArgumentException("Between expects array");

        var arr = json.EnumerateArray().ToArray();

        if (arr.Length != 2)
            throw new ArgumentException("Between expects exactly 2 values");

        var min = Expression.Constant(ConvertValue(arr[0], property.Type));
        var max = Expression.Constant(ConvertValue(arr[1], property.Type));

        var gte = Expression.GreaterThanOrEqual(property, min);
        var lte = Expression.LessThanOrEqual(property, max);

        return Expression.AndAlso(gte, lte);
    }

    // ================= NULL/EMPTY =================
    private static BinaryExpression BuildIsEmpty(Expression property)
    {
        if (property.Type != typeof(string))
            throw new ArgumentException("IsEmpty supports only string fields");

        return Expression.Equal(property, Expression.Constant(string.Empty));
    }

    private static BinaryExpression BuildIsNullOrEmpty(Expression property)
    {
        if (property.Type != typeof(string))
            throw new ArgumentException("IsNullOrEmpty supports only string fields");

        var isNull = Expression.Equal(property, Expression.Constant(null, typeof(string)));
        var isEmpty = Expression.Equal(property, Expression.Constant(string.Empty));

        return Expression.OrElse(isNull, isEmpty);
    }

    // ================= PROPERTY PATH =================
    private static Expression BuildPropertyExpression(Expression parameter, string propertyPath)
    {
        var current = parameter;

        foreach (var prop in propertyPath.Split('.'))
        {
            var propertyInfo = current.Type.GetProperty(prop)
                ?? throw new ArgumentException($"Property '{prop}' not found on {current.Type.Name}");

            current = Expression.Property(current, propertyInfo);
        }

        return current;
    }

    // ================= TYPE CONVERSION =================
    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value == null)
            return null;

        if (value is JsonElement json)
        {
            if (targetType == typeof(string))
                return json.GetString();
            if (targetType == typeof(int))
                return json.GetInt32();
            if (targetType == typeof(long))
                return json.GetInt64();
            if (targetType == typeof(bool))
                return json.GetBoolean();
            if (targetType == typeof(double))
                return json.GetDouble();
            if (targetType == typeof(DateTime))
                return json.GetDateTime();

            return JsonSerializer.Deserialize(json.GetRawText(), targetType);
        }

        if (targetType.IsEnum)
            return Enum.Parse(targetType, value.ToString()!, true);

        return Convert.ChangeType(value, targetType, CultureInfo.CurrentCulture);
    }

    // ================= GROUP =================
    public static Expression BuildGroup<T>(
        ParameterExpression param,
        FilterGroup group)
    {
        Expression? andExp = null;
        Expression? orExp = null;

        if (group.And != null)
        {
            foreach (var f in group.And)
            {
                var exp = Build<T>(param, f);
                andExp = andExp == null ? exp : Expression.AndAlso(andExp, exp);
            }
        }

        if (group.Or != null)
        {
            foreach (var f in group.Or)
            {
                var exp = Build<T>(param, f);
                orExp = orExp == null ? exp : Expression.OrElse(orExp, exp);
            }
        }

        if (andExp != null && orExp != null)
            return Expression.OrElse(andExp, orExp);

        return andExp ?? orExp!;
    }
}
