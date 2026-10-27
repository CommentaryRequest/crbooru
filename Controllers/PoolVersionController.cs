using Microsoft.AspNetCore.Mvc;
using CRbooru.Services;
using CRbooru.Models;

namespace CRbooru.Controllers;

[Route("pool_versions")]
public class PoolVersionController : Controller
{
    private readonly PoolVersionService _service;
    private readonly PoolService _pools;
    private readonly UserService _users;

    public PoolVersionController(PoolVersionService service, PoolService pools, UserService users)
    {
        _service = service;
        _pools = pools;
        _users = users;
    }

    public async Task<IActionResult> Index(int? poolId, int? userId)
    {
        var poolVersions = _service.List(poolId, userId);
        var pool = poolId == null ? null : await _pools.GetAsync((int)poolId);
        var user = userId == null ? null : await _users.Get((int)userId);

        var viewModel = new PoolVersionsViewModel {
            Versions = poolVersions,
            Pool = pool,
            User = user
        };

        return View(viewModel);
    }
}
