namespace CRbooru.Logical;

public enum SearchTermType
{
    Tag,
    Rating,
    Id,
    User
}

public class SearchTerm
{
    public SearchTermType Type { get; set; }
    public string Value { get; set; }
    public bool IsNegated { get; set; }

    public SearchTerm(SearchTermType type, string value, bool isNegated)
    {
        Type = type;
        Value = value;
        IsNegated = isNegated;
    }
}

public static class QueryParser
{
    private static SearchTerm? parseToken(string token)
    {
        token = token.ToLower();

        bool isNegated = token.StartsWith('-');
        if (isNegated) {
            if (token.Length == 1) {
                return null;
            }
            token = token[1..];
        }

        if (token.StartsWith("rating:")) {
            return new SearchTerm(SearchTermType.Rating, token[7..], isNegated);
        }
        if (token.StartsWith("id:")) {
            return new SearchTerm(SearchTermType.Id, token[3..], isNegated);

        }
        if (token.StartsWith("user:")) {
            return new SearchTerm(SearchTermType.User, token[5..], isNegated);
        }
        return new SearchTerm(SearchTermType.Tag, token, isNegated);
    }

    public static List<SearchTerm> ParseQuery(string query)
    {
        var tokens = TagParser.SplitTags(query);
        var terms = new List<SearchTerm>();

        foreach (var token in tokens) {
            SearchTerm? term = parseToken(token);
            if (term != null) {
                terms.Add(term);
            }
        }

        return terms;
    }
}
