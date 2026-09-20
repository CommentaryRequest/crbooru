using Microsoft.AspNetCore.Mvc;
using CRbooru.Services;
using CRbooru.Models;

namespace CRbooru.Controllers;

[Route("posts")]
public class PostController : Controller
{
    private readonly PostService _service;

    public PostController(PostService service)
    {
        _service = service;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Show(int id)
    {
        var post = await _service.Get(id);
        if (post == null) {
            return NotFound();
        }

        return View(post);
    }

    [HttpPost("new")]
    public async Task<IActionResult> Create(PostFormModel model)
    {
        throw new NotImplementedException();
    }
}
