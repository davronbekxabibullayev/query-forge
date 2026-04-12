namespace QueryForge.Abstractions;

public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public List<FilterRule>? Filters { get; set; }
    public List<SortRule>? Sorts { get; set; }
}
