using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;
using System.Collections.Generic;

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

    public async Task<ICollection<Tag>> ResolveTags(IEnumerable<string> names)
    {
        var tags = new List<Tag>();

        foreach (var name in names) {
            var tag = await ByName(name);
            if (tag == null) {
                tag = new Tag(name, 0, TagCategory.General, false);
                _context.Tags.Add(tag);
            }

            tags.Add(tag);
        }

        await _context.SaveChangesAsync();
        return tags;
    }
}
