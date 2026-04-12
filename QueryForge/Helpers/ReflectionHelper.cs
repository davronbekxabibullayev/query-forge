namespace QueryForge.Helpers;

using System.Reflection;

public static class ReflectionHelper
{
    public static PropertyInfo GetProperty(Type type, string propertyName)
    {
        var prop = type.GetProperty(propertyName,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            ?? throw new Exception($"Property '{propertyName}' not found on {type.Name}");

        return prop;
    }
}
