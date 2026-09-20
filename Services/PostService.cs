using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;

namespace CRbooru.Services;

public class PostService
{
    private readonly CRbooruContext _context;
    private readonly TagService _tags;

    public PostService(CRbooruContext context, TagService tags)
    {
        _context = context;
        _tags = tags;
    }

    public Task<Post> Get(int id)
    {
        return _context.Posts
            .Include(p => p.Uploader)
            .Include(p => p.MediaAsset)
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task UpdateTags(Post post, IEnumerable<string> tagList)
    {
        var newTags = await _tags.ResolveTags(tagList);

        var removedTags = post.Tags.Except(newTags).ToList();
        var addedTags = newTags.Except(post.Tags).ToList();

        foreach (var tag in addedTags) {
            tag.PostCount++;
        }

        foreach (var tag in removedTags) {
            tag.PostCount--;
        }

        // If there are no tags left, add tagme.
        if (newTags.Count() == 0) {
            newTags = await _tags.ResolveTags(["tagme"]);
            newTags.ElementAt(0).PostCount++;
        }

        post.Tags = newTags;
        await _context.SaveChangesAsync();
    }
}
