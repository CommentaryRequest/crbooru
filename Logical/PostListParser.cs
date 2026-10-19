namespace CRbooru.Logical;

public static class PostListParser
{
    public static HashSet<int> ParsePostList(string list)
    {
        if (string.IsNullOrWhiteSpace(list)) {
            return new();
        }

        return list
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse)
            .ToHashSet();
    }
}
