using Microsoft.AspNetCore.Mvc;
using CRbooru.Services;
using CRbooru.Models;

namespace CRbooru.Controllers;

[Route("tags")]
public class TagController : Controller
{
    private readonly TagService _service;

    public TagController(TagService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var tags = await _service.ListAsync();
        return View(tags);
    }
}
