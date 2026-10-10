using CRbooru.Models;
using System.Linq;
using System.Text;

namespace CRbooru.Logical;

public static class TagString
{
    private static void addTags(StringBuilder sb, IEnumerable<Tag> tags)
    {
        foreach (var tag in tags) {
            sb.Append($"{tag.Name} ");
        }
        sb.AppendLine();
    }

    public static string Build(ICollection<Tag> tags)
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

    public static string[] Split(string tagString)
    {
        return tagString.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
