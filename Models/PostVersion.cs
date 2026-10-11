namespace CRbooru.Models;

public class PostVersion
{
    public int Id { get; private set; }
    public int PostId { get; private set; }
    public string[]? AddedTags { get; set; }
    public string[]? RemovedTags { get; set; }
    public PostRating? NewRating { get; set; }
    public string? NewSource { get; set; }

    public PostVersion(int postId, string[]? addedTags, string[]? removedTags, PostRating? newRating, string? newSource)
    {
        PostId = postId;
        AddedTags = addedTags;
        RemovedTags = removedTags;
        NewRating = newRating;
        NewSource = newSource;
    }
}
