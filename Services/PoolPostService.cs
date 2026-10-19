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
        Console.WriteLine($"ADD POOL POST ASYNC (pool {poolId}, post {postId})");
        _context.PoolPosts.Add(new PoolPost{
            PoolId = poolId,
            PostId = postId
        });
        await _context.SaveChangesAsync();
    }

    public async Task RemovePoolPostAsync(int poolId, int postId)
    {
        Console.WriteLine($"REMOVE POOL POST ASYNC (pool {poolId}, post {postId})");
        await _context.PoolPosts
            .Where(p => p.PoolId == poolId && p.PostId == postId)
            .ExecuteDeleteAsync();
        await _context.SaveChangesAsync();
    }

    public async Task<List<int>> GetPostPools(int postId)
    {
        return await _context.PoolPosts
            .Where(p => p.PostId == postId)
            .Select(p => p.PoolId)
            .ToListAsync();
    }
}
