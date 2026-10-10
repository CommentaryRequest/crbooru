using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using CRbooru.Data;
using CRbooru.Models;
using CRbooru.Logical;

namespace CRbooru.Services;

public class PostService
{
    private enum MetaTagType
    {
        Rating
    }

    private static readonly Dictionary<string, MetaTagType> META_TAG_PREFIXES = new() {
        ["rating:"] = MetaTagType.Rating
    };

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

    private IEnumerable<string> applyMetaTags(Post post, IEnumerable<string> tagList)
    {
        var metaTags = new List<string>();
        foreach (var tag in tagList) {
            var prefix = META_TAG_PREFIXES.Keys.FirstOrDefault(p => tag.StartsWith(p, StringComparison.OrdinalIgnoreCase));
            if (prefix == null) {
                continue;
            }

            metaTags.Add(tag);
            
            var type = META_TAG_PREFIXES[prefix];
            var value = tag[prefix.Length..];
            switch (type) {
                case MetaTagType.Rating:
                    post.Rating = TagString.ParseRatingValue(value) ?? post.Rating;
                    break;
            }
        }
        return metaTags;
    }

    public async Task UpdateTags(Post post, IEnumerable<string> tagList)
    {
        var metaTags = applyMetaTags(post, tagList);
        tagList = tagList.Except(metaTags);

        var negatedTags = await _tags.ResolveTags(tagList.Where(tag => tag.StartsWith('-')).Select(tag => tag[1..]).Where(tag => tagList.Contains(tag)), false);
        var negatedTagsNames = negatedTags.Select(tag => tag.Name);
        var newTags = await _tags.ResolveTags(tagList.Where(tag => !tag.StartsWith('-')).Except(negatedTagsNames));

        var removedTags = post.Tags.Except(newTags).ToList();
        var addedTags = newTags.Except(post.Tags).Except(removedTags).ToList();

        foreach (var tag in addedTags) {
            tag.PostCount++;
        }

        foreach (var tag in removedTags) {
            if (tag.PostCount == 0) continue;
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

    public async Task EditAsync(Post post, PostFormModel form)
    {
        post.Rating = form.Rating;
        post.Source = form.Source;
        await UpdateTags(post, TagString.Split(form.TagString));
        await _context.SaveChangesAsync();
    }

    public async Task<Post> CreateAsync(User uploader, PostFormModel form)
    {
        var mediaAsset = await _mediaAssets.Get(form.MediaAssetId)!;
        var upload = await _uploads.Get(form.UploadId)!;

        string[] tags = TagString.Split(form.TagString);
        Post post = new Post(uploader, mediaAsset, form.Rating, form.Source);
        UpdateTags(post, tags);
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }
}
