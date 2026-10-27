namespace CRbooru.Models;

public class UserViewModel
{
    public required User User { get; init; }
    public required int PostCount { get; init; }
    public required int PostChangeCount { get; init; }
    public required int PoolChangeCount { get; init; }
    public required List<Post> RecentPosts { get; init; }
}