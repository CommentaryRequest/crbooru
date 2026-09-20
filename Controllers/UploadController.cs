using Microsoft.AspNetCore.Mvc;
using CRbooru.Services;
using CRbooru.Models;

namespace CRbooru.Controllers;

[Route("uploads")]
public class UploadController : Controller
{
    private readonly UploadService _service;
    private readonly UserService _users;

    public UploadController(UploadService service, UserService users)
    {
        _service = service;
        _users = users;
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
            ViewData["Error"] = $"Upload failed: {upload.StatusMessage}";
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

        if (currentUser.Id != upload.Uploader.Id) {
            return Unauthorized();
        }

        if (upload.MediaAssets.Count() == 1) {
            var viewModel = new UploadViewModel(upload, upload.MediaAssets[0]);
            return View("ShowSingle", viewModel);
        } else if (upload.MediaAssets.Count() == 0) {
            var viewModel = new UploadViewModel(upload, null);
            return View("ShowSingle", viewModel);
        }

        throw new NotImplementedException();
    }
}
