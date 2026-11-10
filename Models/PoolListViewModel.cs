namespace CRbooru.Models;

public class PoolListViewModel : IPaginatedViewModel
{
    public required List<Pool> Pools { get; init; }
    public required PaginationInfo PaginationInfo { get; init; }
    public Dictionary<string, string> RouteValues { get; init; } = new();
}