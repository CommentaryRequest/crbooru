namespace CRbooru.Models.ViewModels;

public class PostVersionListViewModel : IPaginatedViewModel
{
    public required List<PostVersion> Versions { get; init; }
    public int? PostId { get; init; }
    public User? User { get; init; }
    public required PaginationInfo PaginationInfo { get; init; }
    public Dictionary<string, string> RouteValues { get; init; } = new();
}