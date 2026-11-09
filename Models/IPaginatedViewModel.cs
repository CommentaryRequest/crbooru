namespace CRbooru.Models;

public interface IPaginatedViewModel
{
    public const int DEFAULT_PAGE_LIMIT = 20;
    
    public int Page { get; init; }
    public int TotalPages { get; init; }
    public int Limit { get; init; }
    public Dictionary<string, string> RouteValues { get; init; }
}