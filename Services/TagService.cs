using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;

namespace CRbooru.Services;

public class TagService
{
    private readonly CRbooruContext _context;

    public TagService(CRbooruContext context)
    {
        _context = context;
    }

    public Task<Tag> ByName(string name)
    {
        return _context.Tags.FirstOrDefaultAsync(t => name.ToLower() == t.Name);
    }
}
