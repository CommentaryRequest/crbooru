using CRbooru.Models;

namespace CRbooru.Logical;

public static class PostVersionUndoer
{
    public static PostVersion Undo(PostVersion version, List<PostVersion> postVersions)
    {
        PostVersion newVersion = new(version);

        // Swap added and removed tags.
        string[] addedTags = newVersion.RemovedTags;
        string[] removedTags = newVersion.AddedTags;
        newVersion.AddedTags = addedTags;
        newVersion.RemovedTags = removedTags;

        // Undo rating if it was changed.
        if (newVersion.NewRating != null) {
            for (int i = version.Number - 2; i >= 0; i--) {
                PostVersion v = postVersions[i];
                if (v.NewRating != null) {
                    newVersion.NewRating = v.NewRating;
                    break;
                }
            }
        }

        if (newVersion.NewRating == version.Post.Rating) {
            newVersion.NewRating = null;
        }

        return newVersion;
    }
}
