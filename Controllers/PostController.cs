using Microsoft.AspNetCore.Mvc;
using CRbooru.Services;
using CRbooru.Models;
using CRbooru.Logical;

namespace CRbooru.Controllers;

[Route("posts")]
public class PostController : Controller
{
    private readonly PostService _service;
    private readonly UserService _users;
    private readonly UploadService _uploads;
    private readonly MediaAssetService _mediaAssets;
    private readonly PoolPostService _poolPosts;

    public PostController(PostService service, UserService users, UploadService uploads, MediaAssetService mediaAssets, PoolPostService poolPosts)
    {
        _service = service;
        _users = users;
        _uploads = uploads;
        _mediaAssets = mediaAssets;
        _poolPosts = poolPosts;
    }

    public async Task<IActionResult> Index(string tags = "", int page = 1)
    {
        page = Math.Max(1, page);
        var posts = new List<Post>();
        var total = 0;
        var limit = PostService.DEFAULT_LIMIT;
        try {
            var parsedQuery = QueryParser.ParseQuery(tags);
            var (postQuery, queryLimit) = await _service.QuerySearchAsync(parsedQuery);
            limit = queryLimit;
            total = postQuery.Count();
            posts = postQuery
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();
        } catch (QueryParserException exc) {
            ViewData["SearchError"] = exc.Message;
            return View("SearchError");
        }

        var totalPages = (int)Math.Ceiling(total / (double)limit);

        var relatedTags = posts
            .SelectMany(p => p.Tags)
            .GroupBy(t => t.Id)
            .Select(g => new { Tag = g.First(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(20)
            .Select(x => x.Tag);

        var model = new PostListViewModel {
            Posts = posts,
            SearchQuery = tags,
            RelatedTags = relatedTags,
            Page = page,
            TotalPages = totalPages
        };
        
        return View(model);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Show(int id)
    {
        var post = await _service.GetFamily(id);
        if (post == null) {
            return NotFound();
        }

        var pools = await _poolPosts.GetPostPools(id);

        PostViewModel model = new(post, pools);

        return View(model);
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

        // Validate tags
        foreach (var tag in TagParser.SplitTags(model.TagString)) {
            try {
                TagParser.ValidateTagName(tag);
            } catch (TagValidationException exc) {
                TempData["Error"] = $"Tag validation failed: {exc.Message}";
                return RedirectToAction("Show", "Upload", new { id = upload.Id });
            }
        }

        // Create the post
        try {
            var post = await _service.CreateAsync(currentUser, model);
            return RedirectToAction("Show", new { id = post.Id });
        } catch (Exception exc) {
            TempData["Error"] = exc.Message;
            // TODO properly log shit
            Console.WriteLine($"error posting: {exc}");
            return RedirectToAction("Show", "Upload", new { id = upload.Id });
        }
    }

    [HttpPost("edit")]
    public async Task<IActionResult> Edit(PostFormModel model)
    {
        var currentUser = await _users.GetCurrentUser(this.User);
        if (currentUser == null) {
            return RedirectToAction("Login", "User");
        }

        var post = await _service.Get(model.PostId);
        if (post == null) {
            return NotFound();
        }

        try {
            await _service.EditAsync(currentUser, post, model);
        } catch (TagValidationException exc) {
            TempData["Error"] = $"Tag validation failed: {exc.Message}";
        }
        return RedirectToAction("Show", new { id = post.Id });
    }
}
