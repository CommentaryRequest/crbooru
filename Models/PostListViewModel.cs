namespace CRbooru.Models;

public class PostListViewModel
{
    public required List<Post> Posts { get; init; }
    public string? SearchQuery { get; init; }
    public required IEnumerable<Tag> RelatedTags { get; init; }
    public required int Page { get; init; }
    public required int TotalPages { get; init; }
}