namespace CRbooru.Models.ViewModels;

public class TagImplicationListViewModel : IPaginatedViewModel
{
    public required List<TagImplication> TagImplications { get; init; }
    public required PaginationInfo PaginationInfo { get; init; }
    public Dictionary<string, string> RouteValues { get; init; } = new();
}