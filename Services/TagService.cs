using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;
using CRbooru.Logical;

namespace CRbooru.Services;

public class TagService
{
    private readonly CRbooruContext _context;

    public TagService(CRbooruContext context)
    {
        _context = context;
    }

    public Task<Tag?> ByName(string name)
    {
        return _context.Tags.FirstOrDefaultAsync(t => name.ToLowerInvariant() == t.Name);
    }

    public async Task<ICollection<Tag>> ResolveTags(IEnumerable<string> names, bool createNew = true)
    {
        IEnumerable<string> uniqueNames = names.Distinct();
        var tags = new List<Tag>();

        foreach (var name in uniqueNames) {
            (TagCategory Category, string NormalizedName)? categoryOverride = TagParser.GetCategoryOverride(name);
            var tagName = categoryOverride?.NormalizedName ?? name;

            var tag = await ByName(tagName);
            if (tag == null) {
                if (createNew) {
                    tag = new Tag(tagName, 0, categoryOverride?.Category ?? TagCategory.General, false);
                    _context.Tags.Add(tag);
                }
            } else {
                tag.Category = categoryOverride?.Category ?? tag.Category;
            }

            if (tag != null) {
                tags.Add(tag);
            }
        }

        await _context.SaveChangesAsync();
        return tags;
    }
}
