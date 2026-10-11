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

    public async Task<List<(PostVersion Version, int Number)>> List(int? postId, int? userId)
    {
        IQueryable<PostVersion> query = _context.PostVersions
            .Include(p => p.User)
            .Include(p => p.Post);
        if (postId != null) {
            query = query.Where(p => p.PostId == postId);
        }
        if (userId != null) {
            query = query.Where(p => p.UserId == userId);
        }

        var versions = await _context.PostVersions
            .Include(v => v.User)
            .Include(v => v.Post)
            .Where(v => v.UserId == userId)
            .ToListAsync();

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
