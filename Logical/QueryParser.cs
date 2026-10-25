namespace CRbooru.Logical;

public enum SearchTermType
{
    Tag,
    Rating,
    Id,
    User,
    Limit,
    Parent,
    Child
}

public record SearchTerm(SearchTermType Type, string Value, bool IsNegated);

public record IdRange(int? Min, int? Max);

public class QueryParserException : Exception
{
    public QueryParserException() : base()
    {
    }

    public QueryParserException(string message) : base(message)
    {
    }

    public QueryParserException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public static class QueryParser
{
    private static SearchTerm? parseToken(string token)
    {
        token = token.ToLowerInvariant();

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
        if (token.StartsWith("limit:")) {
            if (isNegated) {
                throw new QueryParserException("Limit cannot be negated.");
            }
            return new SearchTerm(SearchTermType.Limit, token[6..], false);
        }
        if (token.StartsWith("parent:")) {
            return new SearchTerm(SearchTermType.Parent, token[7..], isNegated);
        }
        if (token.StartsWith("child:")) {
            return new SearchTerm(SearchTermType.Child, token[6..], isNegated);
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

    public static IdRange? ParseIdRange(string value)
    {
        // a..
        if (value.EndsWith("..")) {
            value = value[..^2];
            if (int.TryParse(value, out int min)) {
                return new(min, null);
            }
            return null;
        }

        // ..b
        if (value.StartsWith("..")) {
            value = value[2..];
            if (int.TryParse(value, out int max)) {
                return new(null, max);
            }
            return null;
        }

        // a..b
        if (value.Contains("..")) {
            string[] values = value.Split("..");
            if (values.Length != 2) {
                return null;
            }
            if (int.TryParse(values[0], out int min) && int.TryParse(values[1], out int max)) {
                return new(Math.Min(min, max), Math.Max(min, max));
            }
            return null;
        }

        // >, <, >=, <=
        if (value.StartsWith("<")) {
            bool eq = value[1] == '=';
            value = eq ? value[2..] : value[1..];
            if (int.TryParse(value, out int max)) {
                return new(null, eq ? max : max - 1);
            }
            return null;
        }
        if (value.StartsWith(">")) {
            bool eq = value[1] == '=';
            value = eq ? value[2..] : value[1..];
            if (int.TryParse(value, out int min)) {
                return new(eq ? min : min + 1, null);
            }
            return null;
        }
        return null;
    }
}
