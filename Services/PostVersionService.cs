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

    public List<PostVersion> List(int? postId, int? userId)
    {
        return Query(postId, userId).ToList();
    }

    public async Task Add(PostVersion version)
    {
        if (version.IsEmpty()) {
            return;
        }

        int currentVersion = await _context.PostVersions
            .Where(v => v.PostId == version.PostId)
            .MaxAsync(v => (int?)v.Number) ?? 0;
        version.Number = currentVersion + 1;

        _context.PostVersions.Add(version);
        await _context.SaveChangesAsync();
    }
}
