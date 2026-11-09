namespace CRbooru.Models;

public class TagListViewModel : IPaginatedViewModel
{
    public required List<Tag> Tags { get; init; }
    
    public required PaginationInfo PaginationInfo { get; init; }
    public Dictionary<string, string> RouteValues { get; init; } = new();
}