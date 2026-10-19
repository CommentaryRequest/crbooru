using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;
using CRbooru.Logical;

namespace CRbooru.Services;

public class PoolUpdateService
{
    private readonly CRbooruContext _context;
    private readonly PoolPostService _poolPosts;

    public PoolUpdateService(CRbooruContext context, PoolPostService poolPosts)
    {
        _context = context;
        _poolPosts = poolPosts;
    }

    public async Task AppendAsync(User updater, Pool pool, int postId)
    {
        await _poolPosts.AddPoolPostAsync(pool.Id, postId);
        pool.PostIds.Add(postId);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(User updater, Pool pool, int postId)
    {
        await _poolPosts.RemovePoolPostAsync(pool.Id, postId);
        pool.PostIds.Remove(postId);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePostPoolsAsync(User updater, int postId, PoolUpdate update)
    {
        if (update.ClearPools) {
            var pools = await _poolPosts.GetPostPools(postId);
            foreach (var pool in pools) {
                await RemoveAsync(updater, pool, postId);
            }
            return;
        }

        foreach (var poolId in update.PoolIds) {
            var pool = await _context.Pools.FirstOrDefaultAsync(p => p.Id == poolId);
            if (pool == null) {
                continue;
            }
            await AppendAsync(updater, pool, postId);
        }
    }
}
