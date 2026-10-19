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
    private readonly PoolPostService _poolPosts;

    public PoolService(CRbooruContext context, PostService posts, PoolUpdateService poolUpdate, PoolPostService poolPosts)
    {
        _context = context;
        _posts = posts;
        _poolUpdate = poolUpdate;
        _poolPosts = poolPosts;
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
        foreach (var postId in pool.PostIds) {
            await _poolPosts.AddPoolPostAsync(pool.Id, postId);
        }
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
        var postIds = await ValidatePostList(PostListParser.ParsePostList(form.PostIds));
        var removedPostIds = pool.PostIds.Except(postIds);
        var addedPostIds = postIds.Except(pool.PostIds).Except(removedPostIds);

        foreach (var postId in removedPostIds) {
            await _poolPosts.RemovePoolPostAsync(pool.Id, postId);
        }

        foreach (var postId in addedPostIds) {
            await _poolPosts.AddPoolPostAsync(pool.Id, postId);
        }

        pool.Name = form.Name;
        pool.Description = form.Description;
        pool.PostIds = postIds;

        await _context.SaveChangesAsync();
    }
}
