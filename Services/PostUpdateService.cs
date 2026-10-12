using CRbooru.Data;
using CRbooru.Models;
using CRbooru.Logical;

namespace CRbooru.Services;

public class PostUpdateService
{
    private readonly CRbooruContext _context;
    private readonly TagService _tags;
    private readonly ILogger<PostUpdateService> _logger;

    public PostUpdateService(CRbooruContext context, TagService tags, ILogger<PostUpdateService> logger)
    {
        _context = context;
        _tags = tags;
        _logger = logger;
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
                case MetaTagType.Source:
                    post.Source = string.IsNullOrWhiteSpace(metaTag.Value) || metaTag.Value == "none" ? null : metaTag.Value;
                    break;
            }
        }
        return metaTags;
    }

    public async Task<TagDiff> UpdateTags(Post post, IEnumerable<string> tagList)
    {
        _logger.LogInformation("Update post with tags: {tagList}", tagList);

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

        return new(
            addedTags.Count == 0 ? null : addedTags.Select(t => t.Name).ToArray(),
            removedTags.Count == 0 ? null : removedTags.Select(t => t.Name).ToArray()
        );
    }
}
