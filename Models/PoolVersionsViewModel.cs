namespace CRbooru.Models;

public class PoolVersionsViewModel : IPaginatedViewModel
{
    public required List<PoolVersion> Versions { get; set; }
    public Pool? Pool { get; set; }
    public User? User { get; set; }
    public required PaginationInfo PaginationInfo { get; init; }
    public Dictionary<string, string> RouteValues { get; init; } = new();
}
