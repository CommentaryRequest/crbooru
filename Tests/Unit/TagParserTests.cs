using CRbooru.Logical;
using CRbooru.Models;

namespace CRbooru.Tests.Unit;

public class TagParserTests
{
    [Fact]
    public void BuildTagString_Works()
    {
        Tag[] tags = [
            new Tag("bkub", 0, TagCategory.Artist, false),
            new Tag("original", 0, TagCategory.Copyright, false),
            new Tag("cirno", 0, TagCategory.Character, false),
            new Tag("touhou", 0, TagCategory.Copyright, false),
            new Tag("1girl", 0, TagCategory.General, false),
            new Tag("commentary", 0, TagCategory.Meta, false),
            new Tag("hakurei_reimu", 0, TagCategory.Character, false),
            new Tag("solo", 0, TagCategory.General, false),
            new Tag("dairi", 0, TagCategory.Artist, false),
            new Tag("tagme", 0, TagCategory.Meta, false)
        ];
        string expected = """
        bkub dairi 
        commentary tagme 
        original touhou 
        cirno hakurei_reimu 
        1girl solo 

        """;
        Assert.Equal(expected, TagParser.BuildTagString(tags));
    }

    [Fact]
    public void SplitTags_TagStringPresent_SplitsTags()
    {
        var tagString = "1girl solo blue_archive long_hair";
        string[] expected = ["1girl", "solo", "blue_archive", "long_hair"];
        Assert.Equal(expected, TagParser.SplitTags(tagString));
    }

    [Fact]
    public void SplitTags_TagStringNull_ReturnsEmpty()
    {
        string[] expected = [];
        Assert.Equal(expected, TagParser.SplitTags(null));
    }

    [Theory]
    [InlineData("gen:1girl", TagCategory.General, "1girl")]
    [InlineData("general:1girl", TagCategory.General, "1girl")]
    [InlineData("art:bkub", TagCategory.Artist, "bkub")]
    [InlineData("artist:bkub", TagCategory.Artist, "bkub")]
    [InlineData("char:cirno", TagCategory.Character, "cirno")]
    [InlineData("character:cirno", TagCategory.Character, "cirno")]
    [InlineData("copy:touhou", TagCategory.Copyright, "touhou")]
    [InlineData("copyright:touhou", TagCategory.Copyright, "touhou")]
    [InlineData("meta:tagme", TagCategory.Meta, "tagme")]
    public void GetCategoryOverride_ValidOverride_ReturnsCategoryAndTag(string input, TagCategory expectedCategory, string expectedTag)
    {
        Assert.Equal((expectedCategory, expectedTag), TagParser.GetCategoryOverride(input));
    }

    [Theory]
    [InlineData("notreal:1girl")]
    [InlineData("1girl")]
    public void GetCategoryOverride_InvalidOverride_ReturnsNull(string input)
    {
        Assert.Null(TagParser.GetCategoryOverride(input));
    }

    [Theory]
    [InlineData("g", PostRating.General)]
    [InlineData("s", PostRating.Sensitive)]
    [InlineData("q", PostRating.Questionable)]
    [InlineData("e", PostRating.Explicit)]
    [InlineData("general", PostRating.General)]
    [InlineData("stupid", PostRating.Sensitive)]
    [InlineData("a", null)]
    public void ParseRatingValue_Works(string input, PostRating? expected)
    {
        Assert.Equal(expected, TagParser.ParseRatingValue(input));
    }

    [Theory]
    [InlineData("_tag")]
    [InlineData("tag_")]
    [InlineData("_tag_")]
    [InlineData("tag__name")]
    public void ValidateTagName_InvalidTag_ThrowsException(string tag)
    {
        var action = () => TagParser.ValidateTagName(tag);
        Assert.Throws<TagValidationException>(action);
    }

    [Theory]
    [InlineData("touhou")]
    [InlineData("1girl_solo")]
    public void ValidateTagName_ValidTag_DoesNotThrowException(string tag)
    {
        TagParser.ValidateTagName(tag);
    }

    [Theory]
    [InlineData("rating:s", MetaTagType.Rating, "s")]
    [InlineData("source:none", MetaTagType.Source, "none")]
    [InlineData("parent:1234", MetaTagType.Parent, "1234")]
    [InlineData("pool:1235", MetaTagType.Pool, "1235")]
    public void ParseMetaTag_ValidMetaTag_ParsesMetaTag(string input, MetaTagType expectedType, string expectedValue)
    {
        var expectedMetaTag = new MetaTag(expectedType, expectedValue);
        Assert.Equal(expectedMetaTag, TagParser.ParseMetaTag(input));
    }

    [Theory]
    [InlineData("1girl")]
    [InlineData("invalid:metatag")]
    public void ParseMetaTag_InvalidMetaTag_ReturnsNull(string input)
    {
        Assert.Null(TagParser.ParseMetaTag(input));
    }
}
