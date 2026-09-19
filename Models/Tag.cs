using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CRbooru.Models;

public enum TagCategory : byte
{
    General = 1,
    Artist = 2,
    Character = 3,
    Copyright = 4,
    Meta = 5
}

public class Tag
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public int PostCount { get; set; }
    public TagCategory Category { get; set; }
    public bool IsDeprecated { get; set; }

    public Tag(string name, int postCount, TagCategory category, bool isDeprecated)
    {
        Name = name;
        PostCount = postCount;
        Category = category;
        IsDeprecated = isDeprecated;
    }
}
