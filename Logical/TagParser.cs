using CRbooru.Models;
using System.Linq;
using System.Text;
using System.Collections;

namespace CRbooru.Logical;

public static class TagParser
{
    private static readonly Dictionary<TagCategory, string[]> CATEGORY_PREFIXES = new() {
        [TagCategory.General] = ["general:", "gen:"],
        [TagCategory.Artist] = ["artist:", "art:"],
        [TagCategory.Character] = ["character:", "char:"],
        [TagCategory.Copyright] = ["copyright:", "copy:"],
        [TagCategory.Meta] = ["meta:"]
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
}
