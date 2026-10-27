using CRbooru.Models;
using Microsoft.AspNetCore.Mvc;
using CRbooru.Services;

namespace CRbooru.Controllers;

[Route("post_versions")]
public class PostVersionController : Controller
{
    private readonly PostVersionService _service;
    private readonly UserService _users;

    public PostVersionController(PostVersionService service, UserService users)
    {
        _service = service;
        _users = users;
    }

    public IActionResult Index(int? postId, int? userId)
    {
        var postVersions = _service.List(postId, userId);

        var model = new PostVersionListViewModel {
            Versions = postVersions,
            PostId = postId,
            User = userId == null ? null : postVersions[0].User
        };
        return View(model);
    }

    [Route("{id:int}/undo")]
    public async Task<IActionResult> Undo(int id)
    {
        var currentUser = await _users.GetCurrentUser(this.User);
        if (currentUser == null) {
            return RedirectToAction("Login", "User");
        }

        var postVersion = await _service.Get(id);
        if (postVersion == null) {
            return NotFound();
        }
        await _service.Undo(currentUser, postVersion);
        TempData["Information"] = $"Post version {postVersion.PostId}.{postVersion.Number} undone";
        return RedirectToAction("Index", new {postVersion.PostId});
    }
}
