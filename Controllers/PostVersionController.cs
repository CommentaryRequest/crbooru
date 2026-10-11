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

    public async Task<IActionResult> Index(int? postId, int? userId)
    {
        var postVersions = await _service.List(postId, userId);
        ViewData["PostId"] = postId;
        ViewData["UserId"] = userId;
        if (userId != null) {
            ViewData["User"] = postVersions[0].Version.User;
        }
        return View(postVersions);
    }
}
