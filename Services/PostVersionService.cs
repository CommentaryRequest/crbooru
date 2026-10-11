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

    public List<PostVersion> List(int postId)
    {
        return _context.PostVersions
            .Where(p => p.PostId == postId)
            .Include(p => p.User)
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
