using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;

namespace CRbooru.Services;

public class PostService
{
    private readonly CRbooruContext _context;

    public PostService(CRbooruContext context)
    {
        _context = context;
    }

    public Task<Post> Get(int id)
    {
        return _context.Posts
            .Include(p => p.Uploader)
            .Include(p => p.MediaAsset)
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
