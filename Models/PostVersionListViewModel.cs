namespace CRbooru.Models;

public class PostVersionListViewModel
{
    public required List<PostVersion> Versions { get; init; }
    public int? PostId { get; init; }
    public User? User { get; init; }
}