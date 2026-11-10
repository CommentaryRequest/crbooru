using CRbooru.Models.ViewModels;

namespace CRbooru.Models;

public record PaginationInfo(int Page, int TotalCount, int Limit)
{
    public int ClampedPage() => Math.Clamp(Page, 1, TotalPages());
    public int ClampedLimit() => Math.Clamp(Limit, 1, IPaginatedViewModel.MAX_PAGE_LIMIT);
    public int TotalPages() => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)ClampedLimit()));
    public int SkipAmount() => (ClampedPage() - 1) * ClampedLimit();
}