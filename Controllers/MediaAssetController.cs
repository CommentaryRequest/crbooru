using CRbooru.Logical;
using CRbooru.Models;
using CRbooru.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using CRbooru.Services;
using Microsoft.EntityFrameworkCore;

namespace CRbooru.Controllers;

[Route("media_assets")]
public class MediaAssetController : Controller
{
    private readonly MediaAssetService _service;
    private readonly IWebHostEnvironment _webHostEnv;

    public MediaAssetController(MediaAssetService service, IWebHostEnvironment webHostEnv)
    {
        _service = service;
        _webHostEnv = webHostEnv;
    }

    public async Task<IActionResult> Index(int page = 1, int limit = IPaginatedViewModel.DEFAULT_PAGE_LIMIT)
    {
        var query = _service.Query();
        var total = await query.CountAsync();
        var pagination = new PaginationInfo(page, total, limit);
        var mediaAssets = await query.PaginateAsync(pagination);
        var model = new MediaAssetListViewModel {
            MediaAssets = mediaAssets,
            PaginationInfo = pagination
        };
        return View(model);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Show(int id)
    {
        var mediaAsset = await _service.Get(id);
        if (mediaAsset == null) {
            return NotFound();
        }

        return View(mediaAsset);
    }
}
