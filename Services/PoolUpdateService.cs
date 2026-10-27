using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;
using CRbooru.Logical;

namespace CRbooru.Services;

public class PoolUpdateService
{
    private readonly CRbooruContext _context;
    private readonly PoolPostService _poolPosts;
    private readonly PoolVersionService _poolVersions;

    public PoolUpdateService(CRbooruContext context, PoolPostService poolPosts, PoolVersionService poolVersions)
    {
        _context = context;
        _poolPosts = poolPosts;
        _poolVersions = poolVersions;
    }
    
    private async Task createPoolVersion(User updater, Pool pool)
    {
        var version = new PoolVersion {
            UpdaterId = updater.Id,
            Updater = updater,
            PoolId = pool.Id,
            Pool = pool,
            NewState = pool.PostIds,
            NewName = null,
            NewDescription = null,
            DescriptionChanged = false
        };
        await _poolVersions.AddAsync(version, true);
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
                await createPoolVersion(updater, pool);
            }
            return;
        }

        foreach (var poolId in update.PoolIds) {
            var pool = await _context.Pools.FirstOrDefaultAsync(p => p.Id == poolId);
            if (pool == null) {
                continue;
            }
            await AppendAsync(updater, pool, postId);
            await createPoolVersion(updater, pool);
        }
    }
}
