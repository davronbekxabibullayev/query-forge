namespace QueryForge.Abstractions;

public class FilterGroup
{
    public List<FilterRule>? And { get; set; }
    public List<FilterRule>? Or { get; set; }
}
