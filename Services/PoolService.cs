using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;
using CRbooru.Logical;

namespace CRbooru.Services;

public class PoolService
{
    private readonly CRbooruContext _context;
    private readonly PostService _posts;
    private readonly PoolUpdateService _poolUpdate;

    public PoolService(CRbooruContext context, PostService posts, PoolUpdateService poolUpdate)
    {
        _context = context;
        _posts = posts;
        _poolUpdate = poolUpdate;
    }

    public async Task<List<Pool>> ListAsync()
    {
        return await _context.Pools.ToListAsync();
    }

    public async Task<Pool?> GetAsync(int id)
    {
        return await _context.Pools.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<int>> ValidatePostList(HashSet<int> postIds)
    {
        var existingIds = await _posts.Query()
            .Where(p => postIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToHashSetAsync();

        return postIds.Where(existingIds.Contains).Distinct().ToList();
    }

    public async Task<Pool> CreateAsync(User creator, PoolFormModel form)
    {
        var pool = new Pool {
            Name = form.Name,
            Description = form.Description,
            PostIds = await ValidatePostList(PostListParser.ParsePostList(form.PostIds))
        };

        _context.Pools.Add(pool);
        await _context.SaveChangesAsync();
        return pool;
    }

    public async Task<List<Post>> GetPostsAsync(Pool pool)
    {
        var postsById = await _posts.Query()
            .Where(p => pool.PostIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        return pool.PostIds
            .Where(postsById.ContainsKey)
            .Select(id => postsById[id])
            .ToList();
    }

    public async Task UpdateAsync(User updater, Pool pool, PoolFormModel form)
    {
        pool.Name = form.Name;
        pool.Description = form.Description;
        pool.PostIds = await ValidatePostList(PostListParser.ParsePostList(form.PostIds));

        await _context.SaveChangesAsync();
    }
}
