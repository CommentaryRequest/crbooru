using System.Text;
using CRbooru.Models;

namespace CRbooru.Logical;

public static class EssentialTagStringBuilder
{
    private static string[] namesByCategory(IEnumerable<Tag> tags, TagCategory category)
    {
        return tags.Where(tag => tag.Category == category).Select(tag => tag.UnqualifiedDisplayName()).ToArray();
    }

    private static string shortTagList(string[] tags)
    {
        if (tags.Length == 1) {
            return tags[0];
        }
        return $"{tags[0]} and {tags.Length - 1} more";
    }

    public static string Build(IEnumerable<Tag> tags, int postId)
    {
        var artistTags = namesByCategory(tags, TagCategory.Artist);
        var copyrightTags = namesByCategory(tags, TagCategory.Copyright);
        var characterTags = namesByCategory(tags, TagCategory.Character);

        if (artistTags.Length == 0 && copyrightTags.Length == 0 && characterTags.Length == 0) {
            return $"#{postId}";
        }
        
        var sb = new StringBuilder();

        if (characterTags.Length != 0) {
            sb.Append($"{shortTagList(characterTags)} ");
            if (copyrightTags.Length != 0) {
                sb.Append($"({shortTagList(copyrightTags)}) ");
            }
        } else if (copyrightTags.Length != 0) {
            sb.Append($"{shortTagList(copyrightTags)} ");
        }

        if (artistTags.Length != 0) {
            sb.Append($"drawn by {shortTagList(artistTags)}");
        }

        return sb.ToString().Trim();
    }
}
