using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;

namespace CRbooru.Services;

public class PoolPostService
{
    private readonly CRbooruContext _context;

    public PoolPostService(CRbooruContext context)
    {
        _context = context;
    }

    public async Task AddPoolPostAsync(int poolId, int postId)
    {
        _context.PoolPosts.Add(new PoolPost{
            PoolId = poolId,
            PostId = postId
        });
        await _context.SaveChangesAsync();
    }

    public async Task RemovePoolPostAsync(int poolId, int postId)
    {
        await _context.PoolPosts
            .Where(p => p.PoolId == poolId && p.PostId == postId)
            .ExecuteDeleteAsync();
        await _context.SaveChangesAsync();
    }

    public async Task<List<Pool>> GetPostPools(int postId)
    {
        return await _context.PoolPosts
            .Where(p => p.PostId == postId)
            .Join(
                _context.Pools,
                pp => pp.PoolId,
                p => p.Id,
                (pp, p) => p
            )
            .Where(p => !p.IsDeleted)
            .ToListAsync();
    }
}
