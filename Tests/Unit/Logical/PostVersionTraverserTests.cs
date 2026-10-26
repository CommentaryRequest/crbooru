using CRbooru.Logical;
using CRbooru.Models;

namespace CRbooru.Tests.Unit.Logical;

public class PostVersionTraverserTests
{
    private static readonly List<PostVersion> Versions = new List<PostVersion> {
        new PostVersion(0, 0, [], [], PostRating.Explicit, "new source", true, 10, true),
        new PostVersion(0, 0, [], [], null, null, false, null, false),
        new PostVersion(0, 0, [], [], PostRating.General, null, true, null, true),
        new PostVersion(0, 0, [], [], null, null, false, null, false)
    };

    [Theory]
    [InlineData(1, PostRating.Explicit)]
    [InlineData(2, PostRating.Explicit)]
    [InlineData(4, PostRating.General)]
    public void GetLastRating_Works(int number, PostRating expected)
    {
        Assert.Equal(expected, PostVersionTraverser.GetLastRating(Versions, number));
    }

    [Theory]
    [InlineData(1, "new source")]
    [InlineData(2, "new source")]
    [InlineData(5, null)]
    public void GetLastSource_Works(int number, string? expected)
    {
        Assert.Equal(expected, PostVersionTraverser.GetLastSource(Versions, number));
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(2, 10)]
    [InlineData(4, null)]
    public void GetLastParent_Works(int number, int? expected)
    {
        Assert.Equal(expected, PostVersionTraverser.GetLastParent(Versions, number));
    }
}
