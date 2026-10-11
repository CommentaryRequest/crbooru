namespace CRbooru.Models;

public class PostVersion
{
    public int Id { get; private set; }
    public int PostId { get; private set; }
    public Post Post { get; private set; }
    public int UserId { get; private set; }
    public User User { get; private set; }
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

    public bool IsEmpty()
    {
        return AddedTags == null
            && RemovedTags == null
            && NewRating == null
            && !SourceChanged;
    }
}
