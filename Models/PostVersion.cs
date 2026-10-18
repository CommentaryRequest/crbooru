namespace CRbooru.Models;

public class PostVersion
{
    public int Id { get; private set; }
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int Number { get; set; }
    public string[]? AddedTags { get; set; }
    public string[]? RemovedTags { get; set; }
    public PostRating? NewRating { get; set; }
    public string? NewSource { get; set; }
    public bool SourceChanged { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public PostVersion(int postId, int userId, string[]? addedTags, string[]? removedTags, PostRating? newRating, string? newSource, bool sourceChanged)
    {
        PostId = postId;
        UserId = userId;
        AddedTags = addedTags;
        RemovedTags = removedTags;
        NewRating = newRating;
        NewSource = newSource;
        SourceChanged = sourceChanged;
    }

    public PostVersion(PostVersion other)
    {
        this.PostId = other.PostId;
        this.Post = other.Post;
        this.UserId = other.UserId;
        this.User = other.User;
        this.AddedTags = (string[]?)other.AddedTags?.Clone();
        this.RemovedTags = (string[]?)other.RemovedTags?.Clone();
        this.NewRating = other.NewRating;
        this.SourceChanged = other.SourceChanged;
    }

    public bool IsEmpty()
    {
        return AddedTags == null
            && RemovedTags == null
            && NewRating == null
            && !SourceChanged;
    }
}
