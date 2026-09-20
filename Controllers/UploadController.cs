using Microsoft.AspNetCore.Mvc;
using CRbooru.Services;
using CRbooru.Models;

namespace CRbooru.Controllers;

[Route("uploads")]
public class UploadController : Controller
{
    private readonly UploadService _service;
    private readonly UserService _users;
    private readonly PostService _posts;

    public UploadController(UploadService service, UserService users, PostService posts)
    {
        _service = service;
        _users = users;
        _posts = posts;
    }

    [HttpGet("new")]
    public IActionResult ShowCreate()
    {
        if (!User.Identity?.IsAuthenticated == true) {
            return RedirectToAction("Login", "User");
        }

        return View("Create");
    }

    [HttpPost("new")]
    public async Task<IActionResult> Create(UploadFormModel model)
    {
        var currentUser = await _users.GetCurrentUser(this.User);
        if (currentUser == null) {
            return RedirectToAction("Login", "User");
        }

        var upload = await _service.CreateAsync(currentUser, model.Files);
        if (upload.Status == UploadStatus.Success) {
            return RedirectToAction("Show", new { id = upload.Id });
        } else {
            TempData["Error"] = $"Upload failed: {upload.StatusMessage}";
        }
        return View();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Show(int id)
    {
        var currentUser = await _users.GetCurrentUser(this.User);
        if (currentUser == null) {
            return RedirectToAction("Login", "User");
        }

        var upload = await _service.Get(id);
        if (upload == null) {
            return NotFound();
        }

        if (!currentUser.IsAdmin() && currentUser.Id != upload.Uploader.Id) {
            return Unauthorized();
        }

        if (upload.MediaAssets.Count() == 1) {
            var existingPost = await _posts.ByMediaAsset(upload.MediaAssets[0].Id);
            if (existingPost != null) {
                TempData["Information"] = $"Duplicate of post #{existingPost.Id}";
                return RedirectToAction("Show", "Post", new { id = existingPost.Id });
            }

            var viewModel = new UploadViewModel(upload, upload.MediaAssets[0]);
            return View("ShowSingle", viewModel);
        } else if (upload.MediaAssets.Count() == 0) {
            var viewModel = new UploadViewModel(upload, null);
            return View("ShowSingle", viewModel);
        }

        throw new NotImplementedException();
    }
}
