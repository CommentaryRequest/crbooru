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

    // Does not account for meta tags
    public async Task<TagDiff> UpdateTags(Post post, IEnumerable<string> addedTagNames, IEnumerable<string> removedTagNames)
    {
        var postTagNames = post.Tags.Select(tag => tag.Name);
        addedTagNames = addedTagNames.Select(tag => tag.ToLower()).Except(postTagNames);
        removedTagNames = removedTagNames.Select(tag => tag.ToLower()).Where(tag => postTagNames.Contains(tag));

        var addedTags = await _tags.ResolveTags(addedTagNames);
        var removedTags = await _tags.ResolveTags(removedTagNames);

        foreach (var tag in addedTagNames.Concat(removedTagNames)) {
            TagParser.ValidateTagName(tag);
        }

        foreach (var tag in addedTags) {
            tag.PostCount++;
        }

        foreach (var tag in removedTags) {
            if (tag.PostCount == 0) continue;
            tag.PostCount--;
        }

        post.Tags = post.Tags.Concat(addedTags).ToList();
        post.Tags = post.Tags.Where(tag => !removedTagNames.Contains(tag.Name)).ToList();

        // If there are no tags left, add tagme.
        if (post.Tags.Count() == 0) {
            post.Tags = await _tags.ResolveTags(["tagme"]);
            post.Tags.ElementAt(0).PostCount++;
        }

        await _context.SaveChangesAsync();

        return new(
            addedTags.Count == 0 ? null : addedTags.Select(t => t.Name).ToArray(),
            removedTags.Count == 0 ? null : removedTags.Select(t => t.Name).ToArray()
        );
    }

    public async Task<TagDiff> UpdateTags(Post post, IEnumerable<string> tagList)
    {
        tagList = tagList.Select(tag => tag.ToLower());
        var metaTags = applyMetaTags(post, tagList);
        tagList = tagList.Except(metaTags);

        var negatedTags = await _tags.ResolveTags(tagList.Where(tag => tag.StartsWith('-')).Select(tag => tag[1..]).Where(tag => tagList.Contains(tag)), false);
        var negatedTagsNames = negatedTags.Select(tag => tag.Name);

        var currentTags = post.Tags.Select(tag => tag.Name).AsEnumerable();
        var newTags = tagList.Where(tag => !tag.StartsWith('-')).Except(negatedTagsNames).ToList();
        var removedTags = currentTags.Except(newTags);
        var addedTags = newTags.Except(currentTags).Except(removedTags).ToList();

        return await UpdateTags(post, addedTags, removedTags);
    }
}
