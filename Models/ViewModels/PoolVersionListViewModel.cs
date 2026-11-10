namespace CRbooru.Models.ViewModels;

public class PoolVersionListViewModel : IPaginatedViewModel
{
    public required List<PoolVersion> Versions { get; init; }
    public Pool? Pool { get; init; }
    public User? User { get; init; }
    public required PaginationInfo PaginationInfo { get; init; }
    public Dictionary<string, string> RouteValues { get; init; } = new();
}
