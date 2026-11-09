namespace CRbooru.Models;

public interface IPaginatedViewModel
{
    public int Page { get; init; }
    public int TotalPages { get; init; }
    public Dictionary<string, string> RouteValues { get; init; }
}