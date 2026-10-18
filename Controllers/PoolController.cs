using Microsoft.AspNetCore.Mvc;
using CRbooru.Services;
using CRbooru.Models;

namespace CRbooru.Controllers;

[Route("pools")]
public class PoolController : Controller
{
    private readonly PoolService _service;
    private readonly UserService _users;

    public PoolController(PoolService service, UserService users)
    {
        _service = service;
        _users = users;
    }

    public async Task<IActionResult> Index()
    {
        var pools = await _service.ListAsync();
        return View(pools);
    }

    [Route("new")]
    public async Task<IActionResult> ShowCreate()
    {
        var currentUser = await _users.GetCurrentUser(this.User);
        if (currentUser == null) {
            return RedirectToAction("Login", "User");
        }

        return View();
    }

    [HttpPost("new")]
    public async Task<IActionResult> Create(PoolFormModel model)
    {
        var currentUser = await _users.GetCurrentUser(this.User);
        if (currentUser == null) {
            return RedirectToAction("Login", "User");
        }

        var pool = await _service.CreateAsync(currentUser, model);

        TempData["Information"] = "Pool created";
        return RedirectToAction("Show", new { id = pool.Id });
    }

    [Route("{id:int}")]
    public async Task<IActionResult> Show(int id)
    {
        var pool = await _service.GetAsync(id);
        if (pool == null) {
            return NotFound();
        }

        var posts = await _service.GetPostsAsync(pool);
        var viewModel = new PoolViewModel(pool, posts);

        return View(viewModel);
    }
}
