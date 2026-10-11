using CRbooru.Models;
using System.Text;

namespace CRbooru.Logical;

public class TagValidationException : Exception
{
    public TagValidationException() : base()
    {
    }

    public TagValidationException(string message) : base(message)
    {
    }

    public TagValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public enum MetaTagType
{
    Rating
}


public class MetaTag
{
    public MetaTagType Type { get; set; }
    public string Value { get; set; }

    public MetaTag(MetaTagType type, string value)
    {
        Type = type;
        Value = value;
    }
}

public static class TagParser
{
    private static readonly Dictionary<TagCategory, string[]> CATEGORY_PREFIXES = new() {
        [TagCategory.General] = ["general:", "gen:"],
        [TagCategory.Artist] = ["artist:", "art:"],
        [TagCategory.Character] = ["character:", "char:"],
        [TagCategory.Copyright] = ["copyright:", "copy:"],
        [TagCategory.Meta] = ["meta:"]
    };

    private static readonly Dictionary<string, MetaTagType> META_TAG_PREFIXES = new() {
        ["rating:"] = MetaTagType.Rating
    };

    private static readonly Dictionary<char, PostRating> RATING_MAP = new() {
        ['g'] = PostRating.General,
        ['s'] = PostRating.Sensitive,
        ['q'] = PostRating.Questionable,
        ['e'] = PostRating.Explicit
    };

    private static void addTags(StringBuilder sb, IEnumerable<Tag> tags)
    {
        foreach (var tag in tags) {
            sb.Append($"{tag.Name} ");
        }
        sb.AppendLine();
    }

    public static string BuildTagString(ICollection<Tag> tags)
    {
        IEnumerable<Tag> artistTags = tags.Where(tag => tag.Category == TagCategory.Artist);
        IEnumerable<Tag> metaTags = tags.Where(tag => tag.Category == TagCategory.Meta);
        IEnumerable<Tag> copyrightTags = tags.Where(tag => tag.Category == TagCategory.Copyright);
        IEnumerable<Tag> characterTags = tags.Where(tag => tag.Category == TagCategory.Character);
        IEnumerable<Tag> generalTags = tags.Where(tag => tag.Category == TagCategory.General);

        var sb = new StringBuilder();

        if (artistTags.Count() != 0) {
            addTags(sb, artistTags);
        }
        if (metaTags.Count() != 0) {
            addTags(sb, metaTags);
        }
        if (copyrightTags.Count() != 0) {
            addTags(sb, copyrightTags);
        }
        if (characterTags.Count() != 0) {
            addTags(sb, characterTags);
        }
        if (generalTags.Count() != 0) {
            addTags(sb, generalTags);
        }

        return sb.ToString();
    }

    public static string[] SplitTags(string tagString)
    {
        return tagString.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public static (TagCategory, string)? GetCategoryOverride(string tag)
    {
        foreach (var (category, prefixes) in CATEGORY_PREFIXES) {
            var prefix = prefixes.FirstOrDefault(p => tag.StartsWith(p, StringComparison.OrdinalIgnoreCase));

            if (prefix == null) {
                continue;
            }

            return (category, tag[prefix.Length..]);
        }
        return null;
    }

    public static PostRating? ParseRatingValue(string value)
    {
        if (RATING_MAP.TryGetValue(char.ToLower(value[0]), out PostRating rating)) {
            return rating;
        }

        return null;
    }

    public static void ValidateTagName(string tag)
    {
        if (tag.StartsWith('_') || tag.EndsWith('_')) {
            throw new TagValidationException($"'{tag}' may not begin or end with an underscore");
        }

        if (tag.Contains("__")) {
            throw new TagValidationException($"'{tag}' may not contain consecutive underscores");
        }
    }

    public static MetaTag? ParseMetaTag(string tag)
    {
        var prefix = META_TAG_PREFIXES.Keys.FirstOrDefault(p => tag.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        if (prefix == null) {
            return null;
        }

        var type = META_TAG_PREFIXES[prefix];
        var value = tag[prefix.Length..];
        return new MetaTag(type, value);
    }
}
