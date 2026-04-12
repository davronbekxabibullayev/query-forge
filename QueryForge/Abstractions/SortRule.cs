namespace QueryForge.Abstractions;

public class SortRule
{
    public string Field { get; set; } = default!;
    public bool Desc { get; set; }
}
