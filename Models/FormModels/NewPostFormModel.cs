namespace CRbooru.Models.FormModels;

public class NewPostFormModel : PostFormModel
{
    public string? OriginalTitle { get; init; }
    public string? OriginalDescription { get; init; }
    public string? TranslatedTitle { get; init; }
    public string? TranslatedDescription { get; init; }

    public bool HasCommentary() =>
        OriginalTitle != null ||
        OriginalDescription != null ||
        TranslatedTitle != null ||
        TranslatedDescription != null;
}