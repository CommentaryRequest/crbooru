namespace CRbooru.Models;

public enum PostRating : byte
{
    General,
    Sensitive,
    Questionable,
    Explicit
}

public class Post
{
    public int Id { get; private set; }
    public User Uploader { get; private set; } = null!;
    public MediaAsset MediaAsset { get; init; } = null!;
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public PostRating Rating { get; set; } = PostRating.General;
    public string? Source { get; set; } = "";
    public int? ParentId { get; set; }
    public Post? Parent { get; set; }
    public ICollection<Post> Children { get; set; } = [];
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public ArtistCommentary? Commentary { get; set; }

    private Post()
    {
    }

    public Post(User uploader, MediaAsset mediaAsset, PostRating rating, string? source, int? parentId)
    {
        Uploader = uploader;
        MediaAsset = mediaAsset;
        Tags = new List<Tag>();
        Rating = rating;
        Source = source;
        ParentId = parentId;
    }
}
