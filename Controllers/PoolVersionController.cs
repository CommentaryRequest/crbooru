using CRbooru.Logical;
using Microsoft.AspNetCore.Mvc;
using CRbooru.Services;
using CRbooru.Models;
using CRbooru.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IActionResult> Index(int? poolId, int? userId, int page = 1, int limit = IPaginatedViewModel.DEFAULT_PAGE_LIMIT)
    {
        var query = _service.Query(poolId, userId)
            .OrderByDescending(p => p.Id);
        var totalCount = await query.CountAsync();
        var pagination = new PaginationInfo(page, totalCount, limit);
        var poolVersions = await query.PaginateAsync(pagination);
        var pool = poolId == null ? null : await _pools.GetAsync((int)poolId);
        var user = userId == null ? null : await _users.Get((int)userId);

        var viewModel = new PoolVersionListViewModel {
            Versions = poolVersions,
            Pool = pool,
            User = user,
            PaginationInfo = pagination,
        };
        if (poolId != null) {
            viewModel.RouteValues["poolid"] = poolId.ToString()!;
        }
        if (userId != null) {
            viewModel.RouteValues["userid"] = userId.ToString()!;
        }

        return View(viewModel);
    }
}
