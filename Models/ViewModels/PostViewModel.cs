namespace CRbooru.Models.ViewModels;

public record PostViewModel(Post Post, List<Pool> Pools, ArtistCommentary? Commentary);
