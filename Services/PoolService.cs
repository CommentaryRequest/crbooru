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
    private readonly PoolVersionService _poolVersions;

    public PoolService(CRbooruContext context, PostService posts, PoolUpdateService poolUpdate, PoolPostService poolPosts, PoolVersionService poolVersions)
    {
        _context = context;
        _posts = posts;
        _poolUpdate = poolUpdate;
        _poolPosts = poolPosts;
        _poolVersions = poolVersions;
    }

    private static void validatePoolName(PoolFormModel form)
    {
        if (string.IsNullOrWhiteSpace(form.Name)) {
            throw new ArgumentException("Pool name cannot be empty");
        }
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
        validatePoolName(form);

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

        var version = new PoolVersion {
            UpdaterId = creator.Id,
            PoolId = pool.Id,
            NewState = pool.PostIds,
            NewName = pool.Name,
            NewDescription = pool.Description,
            DescriptionChanged = true
        };
        await _poolVersions.AddAsync(version);

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
        validatePoolName(form);

        var postIds = await ValidatePostList(PostListParser.ParsePostList(form.PostIds));
        var removedPostIds = pool.PostIds.Except(postIds);
        var addedPostIds = postIds.Except(pool.PostIds).Except(removedPostIds);

        foreach (var postId in removedPostIds) {
            await _poolPosts.RemovePoolPostAsync(pool.Id, postId);
        }

        foreach (var postId in addedPostIds) {
            await _poolPosts.AddPoolPostAsync(pool.Id, postId);
        }

        var descriptionChanged = pool.Description != form.Description;
        var version = new PoolVersion {
            UpdaterId = updater.Id,
            PoolId = pool.Id,
            Pool = pool,
            NewState = pool.PostIds.SequenceEqual(postIds) ? null : postIds,
            NewName = form.Name != pool.Name ? form.Name : null,
            NewDescription = descriptionChanged ? form.Description : null,
            DescriptionChanged = descriptionChanged
        };
        await _poolVersions.AddAsync(version);

        pool.Name = form.Name;
        pool.Description = form.Description;
        pool.PostIds = postIds;

        await _context.SaveChangesAsync();
    }
}
