using CRbooru.Logical;

namespace CRbooru.Models;

public class PostListViewModel : IPaginatedViewModel
{
    public required List<Post> Posts { get; init; }
    public string? SearchQuery { get; init; }
    public required IEnumerable<Tag> RelatedTags { get; init; }
    public required PaginationInfo PaginationInfo { get; init; }
    public required Dictionary<string, string> RouteValues { get; init; }
}