using Microsoft.AspNetCore.Mvc;
using CRbooru.Services;

namespace CRbooru.Controllers;

[Route("post_versions")]
public class PostVersionController : Controller
{
    private readonly PostVersionService _service;

    public PostVersionController(PostVersionService service)
    {
        _service = service;
    }

    public IActionResult Index(int postId)
    {
        var postVersions = _service.List(postId);
        ViewData["PostId"] = postId;
        return View(postVersions);
    }
}
