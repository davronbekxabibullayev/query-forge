namespace QueryForge.Abstractions;

public static class FilterLogic
{
    public const string And = "And";
    public const string Or = "Or";
}

public class FilterRule
{
    public required string Field { get; set; }
    public required object Value { get; set; }
    public required string Operator { get; set; }
    public string Logic { get; set; } = FilterLogic.And;
}
