namespace CRbooru.Models;

public class UserListViewModel : IPaginatedViewModel
{
    public required List<User> Users { get; init; }
    public required PaginationInfo PaginationInfo { get; init; }
    public Dictionary<string, string> RouteValues { get; init; } = new();
}