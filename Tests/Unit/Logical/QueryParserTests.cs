using CRbooru.Logical;

namespace CRbooru.Tests.Unit.Logical;

public class QueryParserTests
{
    public static IEnumerable<object?[]> IdRangeCases =>
    [
        ["1", null],
        ["a", null],
        ["1..", new IdRange(1, null)],
        ["a..", null],
        ["..1", new IdRange(null, 1)],
        ["..a", null],
        ["1..2", new IdRange(1, 2)],
        ["2..1", new IdRange(1, 2)],
        ["a..2", null],
        ["1..a", null],
        ["a..b", null],
        ["<2", new IdRange(null, 1)],
        ["<=2", new IdRange(null, 2)],
        ["<a", null],
        ["<=a", null],
        [">2", new IdRange(3, null)],
        [">=2", new IdRange(2, null)],
        [">a", null],
        [">=a", null]
    ];

    [Theory]
    [MemberData(nameof(IdRangeCases))]
    public void ParseIdRange_Works(string input, IdRange? expected)
    {
        var result = QueryParser.ParseIdRange(input);
        Assert.Equal(expected, result);
    }

    public static IEnumerable<object[]> ParseQueryCases =>
    [
        // Empty input
        ["", Array.Empty<SearchTerm>()],
        [" ", Array.Empty<SearchTerm>()],

        // Plain tags
        ["cat", new[] { new SearchTerm(SearchTermType.Tag, "cat", false) }],
        ["cat dog", new[]
        {
            new SearchTerm(SearchTermType.Tag, "cat", false),
            new SearchTerm(SearchTermType.Tag, "dog", false)
        }],

        // Case normalization
        ["CAT", new[] { new SearchTerm(SearchTermType.Tag, "cat", false) }],
        ["RaTiNg:Q", new[] { new SearchTerm(SearchTermType.Rating, "q", false) }],

        // Negation
        ["-cat", new[] { new SearchTerm(SearchTermType.Tag, "cat", true) }],
        ["-", Array.Empty<SearchTerm>()],

        // Metatags
        ["rating:g", new[] { new SearchTerm(SearchTermType.Rating, "g", false) }],
        ["id:123", new[] { new SearchTerm(SearchTermType.Id, "123", false) }],
        ["user:alice", new[] { new SearchTerm(SearchTermType.User, "alice", false) }],
        ["limit:50", new[] { new SearchTerm(SearchTermType.Limit, "50", false) }],
        ["parent:123", new[] { new SearchTerm(SearchTermType.Parent, "123", false) }],
        ["child:456", new[] { new SearchTerm(SearchTermType.Child, "456", false) }],

        // Negated metatags
        ["-rating:e", new[] { new SearchTerm(SearchTermType.Rating, "e", true) }],
        ["-id:123", new[] { new SearchTerm(SearchTermType.Id, "123", true) }],
        ["-user:alice", new[] { new SearchTerm(SearchTermType.User, "alice", true) }],
        ["-parent:123", new[] { new SearchTerm(SearchTermType.Parent, "123", true) }],
        ["-child:456", new[] { new SearchTerm(SearchTermType.Child, "456", true) }],

        // Multiple terms, preserving order
        ["cat rating:s -dog", new[]
        {
            new SearchTerm(SearchTermType.Tag, "cat", false),
            new SearchTerm(SearchTermType.Rating, "s", false),
            new SearchTerm(SearchTermType.Tag, "dog", true)
        }]
    ];

    [Theory]
    [MemberData(nameof(ParseQueryCases))]
    public void ParseQuery_Works(string input, SearchTerm[] expected)
    {
        Assert.Equal(QueryParser.ParseQuery(input), expected);
    }

    [Theory]
    [InlineData("-limit:10")]
    [InlineData("-LIMIt:10")]
    public void ParseQuery_NegatedLimit_Throws(string input)
    {
        Assert.Throws<QueryParserException>(() => QueryParser.ParseQuery(input));
    }
}
