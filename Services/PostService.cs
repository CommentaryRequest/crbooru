using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;

namespace CRbooru.Services;

public class PostService
{
    private readonly CRbooruContext _context;
    private readonly TagService _tags;
    private readonly MediaAssetService _mediaAssets;
    private readonly UploadService _uploads;

    public PostService(CRbooruContext context, TagService tags, MediaAssetService mediaAssets, UploadService uploads)
    {
        _context = context;
        _tags = tags;
        _mediaAssets = mediaAssets;
        _uploads = uploads;
    }

    public Task<Post> Get(int id)
    {
        return _context.Posts
            .Include(p => p.Tags)
            .Include(p => p.MediaAsset)
            .Include(p => p.Uploader)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public Task<Post> ByMediaAsset(int assetId)
    {
        return _context.Posts
            .Include(p => p.Tags)
            .Include(p => p.MediaAsset)
            .Include(p => p.Uploader)
            .FirstOrDefaultAsync(p => p.MediaAsset.Id == assetId);
    }

    public async Task<List<Post>> ListAsync(int limit)
    {
        return await _context.Posts
            .Include(p => p.Tags)
            .Include(p => p.MediaAsset)
            .Include(p => p.Uploader)
            .OrderByDescending(p => p.Id)
            .Take(limit)
            .ToListAsync();
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

    public async Task<Post> CreateAsync(User uploader, PostFormModel form)
    {
        var mediaAsset = await _mediaAssets.Get(form.MediaAssetId)!;
        var upload = await _uploads.Get(form.UploadId)!;

        string[] tags = form.TagString.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Post post = new Post(uploader, mediaAsset, form.Rating, form.Source);
        UpdateTags(post, tags);
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }
}
