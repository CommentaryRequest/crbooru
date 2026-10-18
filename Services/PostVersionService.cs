using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;
using CRbooru.Logical;

namespace CRbooru.Services;

public class PostVersionService
{
    private readonly CRbooruContext _context;
    private readonly PostUpdateService _postUpdate;

    public PostVersionService(CRbooruContext context, PostUpdateService postUpdate)
    {
        _context = context;
        _postUpdate = postUpdate;
    }

    private IQueryable<PostVersion> getPostVersions()
    {
        return _context.PostVersions
            .Include(v => v.User)
            .Include(v => v.Post)
            .Include(v => v.Post.MediaAsset)
            .Include(v => v.Post.Tags);
    }

    public async Task<PostVersion?> Get(int id)
    {
        return await getPostVersions().FirstOrDefaultAsync(p => p.Id == id);
    }

    public IQueryable<PostVersion> Query(int? postId, int? userId)
    {
        IQueryable<PostVersion> query = getPostVersions();

        if (postId != null) {
            query = query.Where(v => v.PostId == postId);
        }
        if (userId != null) {
            query = query.Where(v => v.UserId == userId);
        }

        return query;
    }

    public List<PostVersion> List(int? postId, int? userId, bool orderAscending = false)
    {
        IQueryable<PostVersion> query = Query(postId, userId);
        query = orderAscending
            ? query.OrderBy(v => v.Id)
            : query.OrderByDescending(v => v.Id);
        return query.ToList();
    }

    public async Task Add(PostVersion version)
    {
        if (version.IsEmpty()) {
            return;
        }

        var versions = List(version.PostId, null, true);
        if (versions.Count() > 0) {
            var lastVersion = versions[^1];
            if (lastVersion.UserId == version.UserId && DateTime.UtcNow - lastVersion.CreatedAt < TimeSpan.FromHours(1)) {
                PostVersion mergedVersion = PostVersionMerger.Merge(lastVersion, version, versions);
                lastVersion.AddedTags = mergedVersion.AddedTags;
                lastVersion.RemovedTags = mergedVersion.RemovedTags;
                lastVersion.NewRating = mergedVersion.NewRating;
                await _context.SaveChangesAsync();
                return;
            }
        }

        int currentVersion = await _context.PostVersions
            .Where(v => v.PostId == version.PostId)
            .MaxAsync(v => (int?)v.Number) ?? 0;
        version.Number = currentVersion + 1;

        _context.PostVersions.Add(version);
        await _context.SaveChangesAsync();
    }

    public async Task Undo(User editor, PostVersion version)
    {
        if (version.Number == 1) {
            return;
        }

        List<PostVersion> postVersions = List(version.PostId, null, true);
        var newVersion = PostVersionUndoer.Undo(version, postVersions);

        // Add user and post information to the new version.
        // PostVersion's copy constructor, used by PostVersionUndoer,
        // removes these fields.
        newVersion.UserId = editor.Id;
        newVersion.User = editor;
        newVersion.PostId = version.Post.Id;
        newVersion.Post = version.Post;

        version.Post.Rating = newVersion.NewRating ?? version.Post.Rating;
        version.Post.Source = newVersion.SourceChanged ? newVersion.NewSource : version.Post.Source;

        var diff = await _postUpdate.UpdateTags(version.Post, newVersion.AddedTags ?? [], newVersion.RemovedTags ?? []);

        // Change the diff to what was actually changed.
        newVersion.AddedTags = diff.Added;
        newVersion.RemovedTags = diff.Removed;

        await Add(newVersion);
    }
}
