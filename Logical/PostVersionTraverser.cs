using CRbooru.Models;

namespace CRbooru.Logical;

public static class PostVersionTraverser
{
    public static PostRating? GetLastRating(List<PostVersion> versions, int number)
    {
        if (number < 2) {
            return versions[^1].NewRating;
        }

        for (int i = number - 2; i >= 0; i--) {
            PostVersion v = versions[i];
            Console.WriteLine($"[traverse] {i} = {v.NewRating}");
            if (v.NewRating != null) {
                return v.NewRating;
            }
        }

        return null;
    }

    public static string? GetLastSource(List<PostVersion> versions, int number)
    {
        if (number < 2) {
            return versions[0].NewSource;
        }

        for (int i = number - 2; i >= 0; i--) {
            PostVersion v = versions[i];
            if (v.SourceChanged) {
                return v.NewSource;
            }
        }

        return null;
    }
}
