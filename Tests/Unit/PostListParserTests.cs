using CRbooru.Logical;

namespace CRbooru.Tests.Unit;

public class PostListParserTests
{
    public static IEnumerable<object[]> PostListCases =>
    [
        ["", new HashSet<int>()],
        ["1 2 3", new HashSet<int> {1, 2, 3}],
        ["3 7 2 9 10 1", new HashSet<int> {3, 7, 2, 9, 10, 1}],
        ["1 1 1 2 2 2 3 3 3", new HashSet<int> {1, 2, 3}],
        ["banana 1 apple", new HashSet<int> {1}]
    ];

    [Theory]
    [MemberData(nameof(PostListCases))]
    public void ParsePostList_Works(string input, HashSet<int> expected)
    {
        Assert.Equal(expected, PostListParser.ParsePostList(input));
    }
}
