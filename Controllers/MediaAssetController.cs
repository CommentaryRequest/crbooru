using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CRbooru.Services;

namespace CRbooru.Controllers;

[Route("media_assets")]
public class MediaAssetController : Controller
{
    private readonly MediaAssetService _service;

    public MediaAssetController(MediaAssetService service)
    {
        _service = service;
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
