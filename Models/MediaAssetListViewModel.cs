namespace CRbooru.Models;

public class MediaAssetListViewModel : IPaginatedViewModel
{
    public required List<MediaAsset> MediaAssets { get; init; }
    public required PaginationInfo PaginationInfo { get; init; }
    public Dictionary<string, string> RouteValues { get; init; } = new();
}