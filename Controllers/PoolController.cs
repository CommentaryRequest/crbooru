using CRbooru.Logical;
using Microsoft.AspNetCore.Mvc;
using CRbooru.Services;
using CRbooru.Models;
using CRbooru.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IActionResult> Index(int page = 1, int limit = IPaginatedViewModel.DEFAULT_PAGE_LIMIT)
    {
        var query = _service.Query();
        var total = await query.CountAsync();
        var pagination = new PaginationInfo(page, total, limit);
        var pools = await query.PaginateAsync(pagination);
        var model = new PoolListViewModel {
            Pools = pools,
            PaginationInfo = pagination
        };
        return View(model);
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

    [Route("{id:int}/edit")]
    public async Task<IActionResult> ShowEdit(int id)
    {
        var currentUser = await _users.GetCurrentUser(this.User);
        if (currentUser == null) {
            return RedirectToAction("Login", "User");
        }

        var pool = await _service.GetAsync(id);
        if (pool == null) {
            return NotFound();
        }

        return View(pool);
    }

    [HttpPost("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id, PoolFormModel form)
    {
        var currentUser = await _users.GetCurrentUser(this.User);
        if (currentUser == null) {
            return RedirectToAction("Login", "User");
        }

        var pool = await _service.GetAsync(id);
        if (pool == null) {
            return NotFound();
        }

        await _service.UpdateAsync(currentUser, pool, form);
        TempData["Information"] = "Pool updated";
        return RedirectToAction("Show", new { id = id });
    }
}
