using CRbooru.Logical;
using CRbooru.Models;

namespace CRbooru.Tests.Unit.Logical;

public class EssentialTagStringBuilderTests
{
    private static string build(IEnumerable<Tag> tags, int postId)
    {
        return EssentialTagStringBuilder.Build(tags, postId);
    }

    [Fact]
    public void Build_NoEssentialTags_PostIdOnly()
    {
        Assert.Equal("#12345", build([], 12345));
    }

    [Fact]
    public void Build_AllSingleEssentialTags_FullString()
    {
        Tag[] tags = [
            new Tag("cirno", 0, TagCategory.Character, false),
            new Tag("touhou", 0, TagCategory.Copyright, false),
            new Tag("bkub", 0, TagCategory.Artist, false)
        ];

        Assert.Equal("cirno (touhou) drawn by bkub", build(tags, 0));
    }

    [Fact]
    public void Build_ArtistTagOnly()
    {
        Tag[] tags = [
            new Tag("bkub", 0, TagCategory.Artist, false)
        ];

        Assert.Equal("drawn by bkub", build(tags, 0));
    }

    [Fact]
    public void Build_CharacterTagOnly()
    {
        Tag[] tags = [
            new Tag("cirno", 0, TagCategory.Character, false)
        ];

        Assert.Equal("cirno", build(tags, 0));
    }

    [Fact]
    public void Build_CopyrightTagOnly()
    {
        Tag[] tags = [
            new Tag("touhou", 0, TagCategory.Character, false)
        ];

        Assert.Equal("touhou", build(tags, 0));
    }

    [Fact]
    public void Build_NoArtistTag()
    {
        Tag[] tags = [
            new Tag("cirno", 0, TagCategory.Character, false),
            new Tag("touhou", 0, TagCategory.Copyright, false)
        ];

        Assert.Equal("cirno (touhou)", build(tags, 0));
    }

    [Fact]
    public void Build_NoCharacterTag()
    {
        Tag[] tags = [
            new Tag("touhou", 0, TagCategory.Copyright, false),
            new Tag("bkub", 0, TagCategory.Artist, false)
        ];

        Assert.Equal("touhou drawn by bkub", build(tags, 0));
    }

    [Fact]
    public void Build_NoCopyrightTag()
    {
        Tag[] tags = [
            new Tag("cirno", 0, TagCategory.Character, false),
            new Tag("bkub", 0, TagCategory.Artist, false)
        ];

        Assert.Equal("cirno drawn by bkub", build(tags, 0));
    }

    [Fact]
    public void Build_MultipleTagsPerCategory_ShortenTagList()
    {
        Tag[] tags = [
            new Tag("bkub", 0, TagCategory.Artist, false),
            new Tag("dairi", 0, TagCategory.Artist, false),
            new Tag("original", 0, TagCategory.Copyright, false),
            new Tag("touhou", 0, TagCategory.Copyright, false),
            new Tag("cirno", 0, TagCategory.Character, false),
            new Tag("daiyousei", 0, TagCategory.Character, false),
        ];

        Assert.Equal("cirno and 1 more (original and 1 more) drawn by bkub and 1 more", build(tags, 0));
    }
}
