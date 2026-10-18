using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;
using CRbooru.Logical;

namespace CRbooru.Services;

public record TagDiff(string[]? Added, string[]? Removed);

public class PostService
{
    private readonly CRbooruContext _context;
    private readonly MediaAssetService _mediaAssets;
    private readonly UploadService _uploads;
    private readonly PostVersionService _versions;
    private readonly PostUpdateService _postUpdate;

    public const int DEFAULT_LIMIT = 20;

    public PostService(CRbooruContext context, MediaAssetService mediaAssets, UploadService uploads, PostVersionService versions, PostUpdateService postUpdate)
    {
        _context = context;
        _mediaAssets = mediaAssets;
        _uploads = uploads;
        _versions = versions;
        _postUpdate = postUpdate;
    }

    private IQueryable<Post> getPosts()
    {
        return _context.Posts
            .Include(p => p.Tags.OrderBy(tag => tag.Name))
            .Include(p => p.MediaAsset)
            .Include(p => p.Uploader)
            .Include(p => p.Children)
            .Include(p => p.Parent);
    }

    private IQueryable<Post> getPostsFamily()
    {
        return _context.Posts
            .AsSplitQuery()
            .Include(p => p.Tags.OrderBy(tag => tag.Name))
            .Include(p => p.MediaAsset)
            .Include(p => p.Uploader)
            .Include(p => p!.Children)
                .ThenInclude(p => p.Tags.OrderBy(tag => tag.Name))
            .Include(p => p!.Children)
                .ThenInclude(p => p.MediaAsset)
            .Include(p => p!.Children)
                .ThenInclude(p => p.Uploader)
            .Include(p => p!.Children)
                .ThenInclude(p => p.Children)
            .Include(p => p.Parent)
                .ThenInclude(p => p.Tags.OrderBy(tag => tag.Name))
            .Include(p => p.Parent)
                .ThenInclude(p => p.MediaAsset)
            .Include(p => p.Parent)
                .ThenInclude(p => p.Uploader)
            .Include(p => p.Parent)
                .ThenInclude(p => p!.Children)
                    .ThenInclude(p => p.Tags.OrderBy(tag => tag.Name))
            .Include(p => p.Parent)
                .ThenInclude(p => p!.Children)
                    .ThenInclude(p => p.MediaAsset)
            .Include(p => p.Parent)
                .ThenInclude(p => p!.Children)
                    .ThenInclude(p => p.Uploader);
    }

    public async Task<Post?> Get(int id)
    {
        return await getPosts().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Post?> GetFamily(int id)
    {
        return await getPostsFamily().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Post?> ByMediaAsset(int assetId)
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

    public async Task<(IQueryable<Post>, int)> QueryAsync(List<SearchTerm> terms)
    {
        int limit = DEFAULT_LIMIT;
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
                case SearchTermType.Parent:
                    if (term.Value == "none") {
                        posts = term.IsNegated
                            ? posts.Where(p => p.ParentId != null)
                            : posts.Where(p => p.ParentId == null);
                    } else if (term.Value == "any") {
                        posts = term.IsNegated
                            ? posts.Where(p => p.ParentId == null)
                            : posts.Where(p => p.ParentId != null);
                    } else if (int.TryParse(term.Value, out int parent)) {
                        posts = term.IsNegated
                            ? posts.Where(p => p.ParentId != parent && p.Id != parent)
                            : posts.Where(p => p.ParentId == parent || p.Id == parent);
                    }
                    break;
                case SearchTermType.Child:
                    if (term.Value == "none") {
                        posts = term.IsNegated
                            ? posts.Where(p => p.Children.Count() != 0)
                            : posts.Where(p => p.Children.Count() == 0);
                    } else if (term.Value == "any") {
                        posts = term.IsNegated
                            ? posts.Where(p => p.Children.Count() == 0)
                            : posts.Where(p => p.Children.Count() != 0);
                    }
                    break;
            }
        }
        return (posts
            .OrderByDescending(p => p.Id), limit);
    }

    public async Task EditAsync(User editor, Post post, PostFormModel form)
    {
        PostRating? oldRating = post.Rating;
        string? oldSource = post.Source;
        int? oldParent = post.ParentId;

        // Apply form fields
        post.Rating = form.Rating;
        post.Source = form.Source;
        post.ParentId = form.Parent;

        // Update the post
        TagDiff diff = await _postUpdate.UpdateTags(post, TagParser.SplitTags(form.TagString));

        if (post.ParentId == post.Id) {
            post.ParentId = null;
        }

        await _context.SaveChangesAsync();

        // Create a new post version
        await _versions.Add(
            new PostVersion(
                post.Id,
                editor.Id,
                diff.Added,
                diff.Removed,
                oldRating == post.Rating ? null : post.Rating,
                oldSource == post.Source ? null : post.Source,
                oldSource != post.Source,
                oldParent == post.ParentId ? null : post.ParentId,
                oldParent != post.ParentId
            )
        );
    }

    public async Task<Post> CreateAsync(User uploader, PostFormModel form)
    {
        var mediaAsset = await _mediaAssets.Get(form.MediaAssetId);
        if (mediaAsset == null) {
            throw new ArgumentException($"Media asset #{form.MediaAssetId} not found");
        }
        var upload = await _uploads.Get(form.UploadId)!;

        string[] tags = TagParser.SplitTags(form.TagString);
        Post post = new Post(uploader, mediaAsset, form.Rating, form.Source, form.Parent);
        TagDiff diff = await _postUpdate.UpdateTags(post, tags);
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        await _versions.Add(new PostVersion(post.Id, uploader.Id, diff.Added, null, form.Rating, form.Source, true, form.Parent, true));

        return post;
    }
}
