using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;

namespace CRbooru.Services;

public class PostVersionService
{
    private readonly CRbooruContext _context;

    public PostVersionService(CRbooruContext context)
    {
        _context = context;
    }

    public IQueryable<PostVersion> Query(int? postId, int? userId)
    {
        IQueryable<PostVersion> query = _context.PostVersions
            .Include(v => v.User)
            .Include(v => v.Post);

        if (postId != null) {
            query = query.Where(v => v.PostId == postId);
        }
        if (userId != null) {
            query = query.Where(v => v.UserId == userId);
        }

        return query;
    }

    public async Task<List<(PostVersion Version, int Number)>> List(int? postId, int? userId)
    {
        var versions = Query(postId, userId).ToList();

        return versions
            .GroupBy(v => v.PostId)
            .SelectMany(group => group
                .OrderBy(v => v.CreatedAt)
                .ThenBy(v => v.Id)
                .Select((version, index) =>
                    (Version: version, Number: index + 1)))
            .ToList();
    }

    public async Task Add(PostVersion version)
    {
        if (version.IsEmpty()) {
            return;
        }

        _context.PostVersions.Add(version);
        await _context.SaveChangesAsync();
    }
}
