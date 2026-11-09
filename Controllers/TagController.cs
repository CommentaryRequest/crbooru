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
        page = Math.Max(1, page); // TODO this thing too
        var query = _service.Query();
        // TODO write some kind of thing that paginates and gives total for a query
        var total = query.Count();
        var tags = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
        var model = new TagListViewModel {
            Tags = tags,
            Page = page,
            Limit = limit,
            TotalPages = (int)Math.Ceiling(total / (double)limit), // TODO write a constructor to calculate this
        };
        return View(model);
    }
}
