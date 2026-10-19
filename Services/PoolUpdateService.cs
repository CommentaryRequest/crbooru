using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;
using CRbooru.Logical;

namespace CRbooru.Services;

public class PoolUpdateService
{
    private readonly CRbooruContext _context;

    public PoolUpdateService(CRbooruContext context)
    {
        _context = context;
    }

    public async Task AppendAsync(User updater, Pool pool, int postId)
    {
        pool.PostIds.Add(postId);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(User updater, Pool pool, int postId)
    {
        pool.PostIds.Remove(postId);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePostPoolsAsync(User updater, int postId, PoolUpdate update)
    {
        if (update.ClearPools) {
            //TODO
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
