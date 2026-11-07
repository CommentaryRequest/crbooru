using Microsoft.AspNetCore.Mvc;
using CRbooru.Models;
using CRbooru.Services;

namespace CRbooru.Controllers;

[Route("/tag_implications")]
public class TagImplicationController : Controller
{
    private readonly TagImplicationService _service;
    private readonly UserService _users;
    private readonly TagService _tags;

    public TagImplicationController(TagImplicationService service, UserService users, TagService tags)
    {
        _service = service;
        _users = users;
        _tags = tags;
    }

    public async Task<IActionResult> Index()
    {
        var implications = await _service.ListAsync();
        return View(implications);
    }

    [Route("new")]
    public async Task<IActionResult> ShowCreate()
    {
        var currentUser = await _users.GetCurrentUser(this.User);
        if (currentUser == null) {
            return RedirectToAction("Login", "User");
        }

        if (!currentUser.IsAdmin()) {
            return Forbid();
        }

        return View();
    }

    [HttpPost("new")]
    public async Task<IActionResult> Create(TagImplicationFormModel form)
    {
        var currentUser = await _users.GetCurrentUser(this.User);
        if (currentUser == null) {
            return RedirectToAction("Login", "User");
        }

        if (!currentUser.IsAdmin()) {
            return Forbid();
        }

        form.Antecedent = form.Antecedent.Trim();
        form.Consequent = form.Consequent.Trim();

        if (string.IsNullOrWhiteSpace(form.Antecedent)) {
            TempData["Error"] = "Missing antecedent tag";
            return RedirectToAction("ShowCreate");
        }

        if (string.IsNullOrWhiteSpace(form.Consequent)) {
            TempData["Error"] = "Missing consequent tag";
            return RedirectToAction("ShowCreate");
        }

        var tags = await _tags.ResolveTags([form.Antecedent, form.Consequent]);
        var antecedent = tags.ElementAt(0);
        var consequent = tags.ElementAt(1);

        try {
            await _service.AddAsync(antecedent, consequent);
        } catch (TagImplicationException exc) {
            TempData["Error"] = exc.Message;
            return RedirectToAction("ShowCreate");
        }

        TempData["Information"] = $"Implied '{form.Antecedent}' to '{form.Consequent}'";
        return RedirectToAction("Index");
    }
}
