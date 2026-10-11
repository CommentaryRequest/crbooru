using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;
using CRbooru.Logical;

namespace CRbooru.Services;

public class PostService
{
    private readonly CRbooruContext _context;
    private readonly TagService _tags;
    private readonly MediaAssetService _mediaAssets;
    private readonly UploadService _uploads;

    public const int DEFAULT_LIMIT = 20;

    public PostService(CRbooruContext context, TagService tags, MediaAssetService mediaAssets, UploadService uploads)
    {
        _context = context;
        _tags = tags;
        _mediaAssets = mediaAssets;
        _uploads = uploads;
    }

    private IQueryable<Post> getPosts()
    {
        return _context.Posts
            .Include(p => p.Tags.OrderBy(tag => tag.Name))
            .Include(p => p.MediaAsset)
            .Include(p => p.Uploader);
    }

    public async Task<Post> Get(int id)
    {
        return await getPosts().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Post> ByMediaAsset(int assetId)
    {
        return await getPosts().FirstOrDefaultAsync(p => p.MediaAsset.Id == assetId);
    }

    public IQueryable<Post> ByUser(int userId)
    {
        return getPosts().Where(p => p.Uploader.Id == userId);
    }

    public async Task<List<Post>> ListAsync(int limit = DEFAULT_LIMIT)
    {
        return await getPosts()
            .OrderByDescending(p => p.Id)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<Post>> QueryAsync(List<SearchTerm> terms, int limit = DEFAULT_LIMIT)
    {
        IQueryable<Post> posts = getPosts();
        foreach (var term in terms) {
            switch (term.Type) {
                case SearchTermType.Tag:
                    posts = term.IsNegated
                        ? posts.Where(p => !p.Tags.Any(t => t.Name == term.Value))
                        : posts.Where(p => p.Tags.Any(t => t.Name == term.Value));
                    break;
                case SearchTermType.Rating:
                    PostRating? rating = TagParser.ParseRatingValue(term.Value);
                    if (rating != null) {
                        posts = term.IsNegated
                            ? posts.Where(p => p.Rating != rating)
                            : posts.Where(p => p.Rating == rating);
                    }
                    break;
                case SearchTermType.Id:
                    IdRange? range = QueryParser.ParseIdRange(term.Value);
                    if (range != null) {
                        posts = term.IsNegated
                            ? posts.Where(p => !(
                                    (range.Min == null || p.Id >= range.Min) &&
                                    (range.Max == null || p.Id <= range.Max)
                                ))
                            : posts.Where(p => (
                                    (range.Min == null || p.Id >= range.Min) &&
                                    (range.Max == null || p.Id <= range.Max)
                                ));
                    } else if (int.TryParse(term.Value, out int id)) {
                        posts = term.IsNegated
                            ? posts.Where(p => p.Id != id)
                            : posts.Where(p => p.Id == id);
                    }
                    break;
                case SearchTermType.User:
                    posts = term.IsNegated
                        ? posts.Where(p => p.Uploader.Name != term.Value)
                        : posts.Where(p => p.Uploader.Name == term.Value);
                    break;
                case SearchTermType.Limit:
                    if (int.TryParse(term.Value, out int l)) {
                        limit = l;
                    }
                    break;
            }
        }
        return await posts
            .OrderByDescending(p => p.Id)
            .Take(limit)
            .ToListAsync();
    }

    private IEnumerable<string> applyMetaTags(Post post, IEnumerable<string> tagList)
    {
        var metaTags = new List<string>();
        foreach (var tag in tagList) {
            var metaTag = TagParser.ParseMetaTag(tag);
            if (metaTag == null) {
                continue;
            }
            metaTags.Add(tag);

            switch (metaTag.Type) {
                case MetaTagType.Rating:
                    post.Rating = TagParser.ParseRatingValue(metaTag.Value) ?? post.Rating;
                    break;
            }
        }
        return metaTags;
    }

    public async Task UpdateTags(Post post, IEnumerable<string> tagList)
    {
        tagList = tagList.Select(tag => tag.ToLower());
        var metaTags = applyMetaTags(post, tagList);
        tagList = tagList.Except(metaTags);

        var negatedTags = await _tags.ResolveTags(tagList.Where(tag => tag.StartsWith('-')).Select(tag => tag[1..]).Where(tag => tagList.Contains(tag)), false);
        var negatedTagsNames = negatedTags.Select(tag => tag.Name);

        foreach (var tag in tagList.Except(negatedTagsNames)) {
            TagParser.ValidateTagName(tag);
        }

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
        await UpdateTags(post, TagParser.SplitTags(form.TagString));
        await _context.SaveChangesAsync();
    }

    public async Task<Post> CreateAsync(User uploader, PostFormModel form)
    {
        var mediaAsset = await _mediaAssets.Get(form.MediaAssetId)!;
        var upload = await _uploads.Get(form.UploadId)!;

        string[] tags = TagParser.SplitTags(form.TagString);
        Post post = new Post(uploader, mediaAsset, form.Rating, form.Source);
        await UpdateTags(post, tags);
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }
}
