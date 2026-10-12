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

        return newVersion;
    }
}
