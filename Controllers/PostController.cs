using Microsoft.AspNetCore.Mvc;
using CRbooru.Services;
using CRbooru.Models;

namespace CRbooru.Controllers;

[Route("posts")]
public class PostController : Controller
{
    private readonly PostService _service;
    private readonly UserService _users;
    private readonly UploadService _uploads;
    private readonly MediaAssetService _mediaAssets;

    public PostController(PostService service, UserService users, UploadService uploads, MediaAssetService mediaAssets)
    {
        _service = service;
        _users = users;
        _uploads = uploads;
        _mediaAssets = mediaAssets;
    }

    public async Task<IActionResult> Index()
    {
        var posts = await _service.ListAsync(20);

        return View(posts);
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
        var currentUser = await _users.GetCurrentUser(this.User);
        if (currentUser == null) {
            return RedirectToAction("Login", "User");
        }

        // Check if the upload exists and the user can access it
        var upload = await _uploads.Get(model.UploadId);
        if (upload == null) {
            return BadRequest();
        }
        if (!currentUser.IsAdmin() && upload.Uploader.Id != currentUser.Id) {
            return Unauthorized();
        }

        // Check if the asset exists
        var mediaAsset = await _mediaAssets.Get(model.MediaAssetId);
        if (mediaAsset == null) {
            return BadRequest();
        }
        
        // Check if this asset has already been uploaded
        var existingPost = await _service.ByMediaAsset(mediaAsset.Id);
        if (existingPost != null) {
            TempData["Information"] = $"Duplicate of post #{existingPost.Id}";
            return RedirectToAction("Show", new { id = existingPost.Id });
        }

        // Create the post
        try {
            var post = await _service.CreateAsync(currentUser, model);
            return RedirectToAction("Show", new { id = post.Id });
        } catch (Exception exc) {
            TempData["Error"] = exc.Message;
            return RedirectToAction("Show", "Upload", new { id = upload.Id });
        }
    }
}
