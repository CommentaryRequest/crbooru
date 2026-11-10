namespace CRbooru.Models.FormModels;

public class NewPostFormModel : PostFormModel
{
    public string? CommentaryOriginalTitle { get; init; }
    public string? CommentaryOriginalDescription { get; init; }
    public string? CommentaryTranslatedTitle { get; init; }
    public string? CommentaryTranslatedDescription { get; init; }

    public bool HasCommentary() =>
        CommentaryOriginalTitle != null ||
        CommentaryOriginalDescription != null ||
        CommentaryTranslatedTitle != null ||
        CommentaryTranslatedDescription != null;
}