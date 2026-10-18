using CRbooru.Models;

namespace CRbooru.Logical;

public static class PostVersionMerger
{
    public static PostVersion Merge(PostVersion oldVersion, PostVersion newVersion, List<PostVersion> postVersions)
    {
        int versionNumber = oldVersion.Number;
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

        if (newVersion.NewRating != null) {
            result.NewRating = newVersion.NewRating;

            var lastRating = PostVersionTraverser.GetLastRating(postVersions, versionNumber);
            if (result.NewRating == lastRating) {
                result.NewRating = null;
            }
        }

        if (newVersion.SourceChanged) {
            result.NewSource = newVersion.NewSource;
            result.SourceChanged = true;

            var lastSource = PostVersionTraverser.GetLastSource(postVersions, versionNumber);
            if (result.NewSource == lastSource) {
                result.NewSource = null;
                result.SourceChanged = false;
            }
        }

        return result;
    }
}
