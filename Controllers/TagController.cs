using CRbooru.Logical;
using Microsoft.AspNetCore.Mvc;
using CRbooru.Services;
using CRbooru.Models;
using Microsoft.EntityFrameworkCore;

namespace CRbooru.Controllers;

[Route("tags")]
public class TagController : Controller
{
    private readonly TagService _service;

    public TagController(TagService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(int page = 1, int limit = IPaginatedViewModel.DEFAULT_PAGE_LIMIT)
    {
        var query = _service.Query();
        var total = query.Count();
        var pagination = new PaginationInfo(page, total, limit);
        var tags = await query
            .Skip(pagination.SkipAmount())
            .Take(pagination.ClampedLimit())
            .ToListAsync();
        var model = new TagListViewModel {
            Tags = tags,
            PaginationInfo = pagination
        };
        return View(model);
    }
}
