using CRbooru.Logical;
using Microsoft.AspNetCore.Mvc;
using CRbooru.Services;
using CRbooru.Models;
using CRbooru.Models.ViewModels;
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
        var total = await query.CountAsync();
        var pagination = new PaginationInfo(page, total, limit);
        var tags = await query.PaginateAsync(pagination);
        var model = new TagListViewModel {
            Tags = tags,
            PaginationInfo = pagination
        };
        return View(model);
    }
}
