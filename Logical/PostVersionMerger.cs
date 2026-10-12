using CRbooru.Models;

namespace CRbooru.Logical;

public static class PostVersionMerger
{
    public static PostVersion Merge(PostVersion oldVersion, PostVersion newVersion)
    {
        PostVersion result = new(oldVersion);

        var oldAddedTags = oldVersion.AddedTags ?? [];
        var oldRemovedTags = oldVersion.RemovedTags ?? [];
        var newAddedTags = newVersion.AddedTags ?? [];
        var newRemovedTags = newVersion.RemovedTags ?? [];

        result.AddedTags = oldAddedTags
            .Union(newAddedTags)
            .Except(oldRemovedTags.Union(newRemovedTags))
            .ToArray();

        result.RemovedTags = oldRemovedTags
            .Union(newRemovedTags)
            .Except(oldAddedTags.Union(newAddedTags))
            .ToArray();

        return result;
    }
}
