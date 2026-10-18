using CRbooru.Models;

namespace CRbooru.Logical;

public static class PostVersionUndoer
{
    public static PostVersion Undo(PostVersion version, List<PostVersion> postVersions)
    {
        PostVersion newVersion = new(version);

        // Swap added and removed tags.
        string[] addedTags = newVersion.RemovedTags ?? [];
        string[] removedTags = newVersion.AddedTags ?? [];
        newVersion.AddedTags = addedTags;
        newVersion.RemovedTags = removedTags;

        // Undo rating if it was changed.
        if (newVersion.NewRating != null) {
            newVersion.NewRating = PostVersionTraverser.GetLastRating(postVersions, version.Number);
        }
        if (newVersion.NewRating == version.Post.Rating) {
            newVersion.NewRating = null;
        }

        // Undo source if it was changed.
        if (newVersion.SourceChanged) {
            newVersion.NewSource = PostVersionTraverser.GetLastSource(postVersions, version.Number);
        }
        if (newVersion.NewSource == version.Post.Source) {
            newVersion.SourceChanged = false;
            newVersion.NewSource = null;
        }

        return newVersion;
    }
}
