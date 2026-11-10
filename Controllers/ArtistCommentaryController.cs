using CRbooru.Models.FormModels;
using CRbooru.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRbooru.Controllers;

[Route("artist_commentaries")]
public class ArtistCommentaryController : Controller
{
    private ArtistCommentaryService _service;
    private UserService _users;

    public ArtistCommentaryController(ArtistCommentaryService service, UserService users)
    {
        _service = service;
        _users = users;
    }

    [HttpPost("create_or_update")]
    public async Task<IActionResult> CreateOrUpdate(int postId, ArtistCommentaryFormModel form)
    {
        var currentUser = await _users.GetCurrentUser(User);
        if (currentUser == null) {
            return RedirectToAction("Login", "User");
        }

        try {
            await _service.CreateOrUpdateAsync(currentUser, postId, form.OriginalTitle, form.OriginalDescription,
                form.TranslatedTitle, form.TranslatedDescription);
        } catch (ArtistCommentaryException exc) {
            TempData["Error"] = exc.Message;
        }

        return RedirectToAction("Show", "Post", new { id = postId});
    }
}