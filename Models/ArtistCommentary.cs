namespace CRbooru.Models;

public class ArtistCommentary
{
    public int Id { get; init; }
    public int PostId { get; init; }
    public Post Post { get; init; } = null!;
    public string? OriginalTitle { get; set; }
    public string? OriginalDescription { get; set; }
    public string? TranslatedTitle { get; set; }
    public string? TranslatedDescription { get; set; }

    public bool HasOriginalTitle() => !string.IsNullOrWhiteSpace(OriginalTitle);
    public bool HasOriginalDescription() => !string.IsNullOrWhiteSpace(OriginalDescription);
    public bool HasTranslatedTitle() => !string.IsNullOrWhiteSpace(TranslatedTitle);
    public bool HasTranslatedDescription() => !string.IsNullOrWhiteSpace(TranslatedDescription);
    
    public bool HasOriginal() => HasOriginalTitle() || HasOriginalDescription();
    public bool HasTranslated() => HasTranslatedTitle() || HasTranslatedDescription();
    public bool Empty() => !HasOriginal() && !HasTranslated();
}