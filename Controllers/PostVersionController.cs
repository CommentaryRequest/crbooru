using CRbooru.Logical;
using CRbooru.Models;
using CRbooru.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using CRbooru.Services;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IActionResult> Index(int? postId, int? userId, int page = 1, int limit = IPaginatedViewModel.DEFAULT_PAGE_LIMIT)
    {
        var query = _service.Query(postId, userId)
            .OrderByDescending(p => p.Id);
        var totalCount = await query.CountAsync();
        var pagination = new PaginationInfo(page, totalCount, limit);
        var postVersions = await query.PaginateAsync(pagination);

        var model = new PostVersionListViewModel {
            Versions = postVersions,
            PostId = postId,
            User = userId == null ? null : postVersions[0].User,
            PaginationInfo = pagination
        };
        if (postId != null) {
            model.RouteValues["postid"] = postId.ToString()!;
        }
        if (userId != null) {
            model.RouteValues["userid"] = userId.ToString()!;
        }
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
