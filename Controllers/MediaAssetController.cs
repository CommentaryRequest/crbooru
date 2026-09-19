using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CRbooru.Data;

namespace CRbooru.Controllers;

[Route("media_assets")]
public class MediaAssetController : Controller
{
    private readonly CRbooruContext _context;

    public MediaAssetController(CRbooruContext context)
    {
        _context = context;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Show(int id)
    {
        var mediaAsset = await _context.MediaAssets.FindAsync(id);
        if (mediaAsset == null) {
            return NotFound();
        }

        ViewData["ImagePath"] = mediaAsset.GetFilePath("/images");
        return View(mediaAsset);
    }
}
