namespace CRbooru.Models;

public class TagListViewModel : IPaginatedViewModel
{
    public required List<Tag> Tags { get; init; }
    
    public required int Page { get; init; }
    public required int TotalPages { get; init; }
    public required int Limit { get; init; }
    public Dictionary<string, string> RouteValues { get; init; } = new();
}