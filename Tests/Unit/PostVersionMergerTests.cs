using CRbooru.Logical;
using CRbooru.Models;

namespace CRbooru.Tests.Unit;

public class PostVersionMergerTests
{
    private PostVersion createVersion(
        int number = 2,
        string[]? addedTags = null,
        string[]? removedTags = null,
        PostRating? newRating = null,
        string? newSource = null,
        bool sourceChanged = false,
        int? newParent = null,
        bool parentChanged = false)
    {
        var version = new PostVersion(
            1, 1,
            addedTags ?? [],
            removedTags ?? [],
            newRating,
            newSource,
            sourceChanged,
            newParent,
            parentChanged);

        version.Number = number;
        return version;
    }

    private static List<PostVersion> createHistory(params PostVersion[] versions)
    {
        return versions.ToList();
    }

    [Fact]
    public void Merge_TagsOnly_CombinesTagChanges()
    {
        var oldVersion = createVersion(
            addedTags: ["cat", "solo"],
            removedTags: ["dog"]);

        var newVersion = createVersion(
            addedTags: ["blue_hair", "cat"],
            removedTags: ["long_hair"]);

        var result = PostVersionMerger.Merge(
            oldVersion, newVersion, createHistory(oldVersion));

        Assert.Equal(
            new[] { "cat", "solo", "blue_hair" },
            result.AddedTags);

        Assert.Equal(
            new[] { "dog", "long_hair" },
            result.RemovedTags);
    }

    [Fact]
    public void Merge_TagAddedAndRemoved_CancelsBothChanges()
    {
        var oldVersion = createVersion(
            addedTags: ["cat"],
            removedTags: ["dog"]);

        var newVersion = createVersion(
            addedTags: ["dog"],
            removedTags: ["cat"]);

        var result = PostVersionMerger.Merge(
            oldVersion, newVersion, createHistory(oldVersion));

        Assert.Empty(result.AddedTags!);
        Assert.Empty(result.RemovedTags!);
    }

    [Fact]
    public void Merge_DuplicateTags_DeduplicatesResults()
    {
        var oldVersion = createVersion(
            addedTags: ["cat", "cat", "solo"]);

        var newVersion = createVersion(
            addedTags: ["cat", "blue_hair", "blue_hair"]);

        var result = PostVersionMerger.Merge(
            oldVersion, newVersion, createHistory(oldVersion));

        Assert.Equal(
            new[] { "cat", "solo", "blue_hair" },
            result.AddedTags);
    }

    [Fact]
    public void Merge_NullTagCollections_TreatsThemAsEmpty()
    {
        var oldVersion = createVersion();
        oldVersion.AddedTags = null;
        oldVersion.RemovedTags = null;

        var newVersion = createVersion();
        newVersion.AddedTags = null;
        newVersion.RemovedTags = null;

        var result = PostVersionMerger.Merge(
            oldVersion, newVersion, createHistory(oldVersion));

        Assert.Empty(result.AddedTags!);
        Assert.Empty(result.RemovedTags!);
    }

    [Fact]
    public void Merge_NewRating_PreservesRatingChange()
    {
        var previous = createVersion(
            number: 1,
            newRating: PostRating.General);

        var oldVersion = createVersion(number: 2);
        var newVersion = createVersion(newRating: PostRating.Explicit);

        var result = PostVersionMerger.Merge(
            oldVersion,
            newVersion,
            createHistory(previous, oldVersion));

        Assert.Equal(PostRating.Explicit, result.NewRating);
    }

    [Fact]
    public void Merge_RatingMatchesLastRating_CancelsChange()
    {
        var previous = createVersion(
            number: 1,
            newRating: PostRating.General);

        var oldVersion = createVersion(number: 2);
        var newVersion = createVersion(newRating: PostRating.General);

        var result = PostVersionMerger.Merge(
            oldVersion,
            newVersion,
            createHistory(previous, oldVersion));

        Assert.Null(result.NewRating);
    }

    [Fact]
    public void Merge_NullRating_PreservesOldRating()
    {
        var oldVersion = createVersion(newRating: PostRating.Sensitive);
        var newVersion = createVersion();

        var result = PostVersionMerger.Merge(
            oldVersion, newVersion, createHistory(oldVersion));

        Assert.Equal(PostRating.Sensitive, result.NewRating);
    }

    [Fact]
    public void Merge_NewSource_PreservesSourceChange()
    {
        var previous = createVersion(
            number: 1,
            newSource: "https://example.com/old",
            sourceChanged: true);

        var oldVersion = createVersion(number: 2);
        var newVersion = createVersion(
            newSource: "https://example.com/new",
            sourceChanged: true);

        var result = PostVersionMerger.Merge(
            oldVersion,
            newVersion,
            createHistory(previous, oldVersion));

        Assert.True(result.SourceChanged);
        Assert.Equal("https://example.com/new", result.NewSource);
    }

    [Fact]
    public void Merge_SourceMatchesLastSource_CancelsChange()
    {
        var previous = createVersion(
            number: 1,
            newSource: "https://example.com/original",
            sourceChanged: true);

        var oldVersion = createVersion(number: 2);
        var newVersion = createVersion(
            newSource: "https://example.com/original",
            sourceChanged: true);

        var result = PostVersionMerger.Merge(
            oldVersion,
            newVersion,
            createHistory(previous, oldVersion));

        Assert.False(result.SourceChanged);
        Assert.Null(result.NewSource);
    }

    [Fact]
    public void Merge_SourceNotChanged_PreservesOldSourceChange()
    {
        var oldVersion = createVersion(
            newSource: "https://example.com/old",
            sourceChanged: true);

        var newVersion = createVersion();

        var result = PostVersionMerger.Merge(
            oldVersion, newVersion, createHistory(oldVersion));

        Assert.True(result.SourceChanged);
        Assert.Equal("https://example.com/old", result.NewSource);
    }

    [Fact]
    public void Merge_NewParent_PreservesParentChange()
    {
        var previous = createVersion(
            number: 1,
            newParent: 10,
            parentChanged: true);

        var oldVersion = createVersion(number: 2);
        var newVersion = createVersion(
            newParent: 20,
            parentChanged: true);

        var result = PostVersionMerger.Merge(
            oldVersion,
            newVersion,
            createHistory(previous, oldVersion));

        Assert.True(result.ParentChanged);
        Assert.Equal(20, result.NewParent);
    }

    [Fact]
    public void Merge_ParentMatchesLastParent_CancelsChange()
    {
        var previous = createVersion(
            number: 1,
            newParent: 10,
            parentChanged: true);

        var oldVersion = createVersion(number: 2);
        var newVersion = createVersion(
            newParent: 10,
            parentChanged: true);

        var result = PostVersionMerger.Merge(
            oldVersion,
            newVersion,
            createHistory(previous, oldVersion));

        Assert.False(result.ParentChanged);
        Assert.Null(result.NewParent);
    }

    [Fact]
    public void Merge_ParentNotChanged_PreservesOldParentChange()
    {
        var oldVersion = createVersion(
            newParent: 10,
            parentChanged: true);

        var newVersion = createVersion();

        var result = PostVersionMerger.Merge(
            oldVersion, newVersion, createHistory(oldVersion));

        Assert.True(result.ParentChanged);
        Assert.Equal(10, result.NewParent);
    }

    [Fact]
    public void Merge_AllFields_MergesAllChanges()
    {
        var oldVersion = createVersion(
            addedTags: ["cat"],
            removedTags: ["dog"],
            newRating: PostRating.Sensitive,
            newSource: "https://example.com/old",
            sourceChanged: true,
            newParent: 10,
            parentChanged: true);

        var newVersion = createVersion(
            addedTags: ["blue_hair"],
            removedTags: ["long_hair"],
            newRating: PostRating.Explicit,
            newSource: "https://example.com/new",
            sourceChanged: true,
            newParent: 20,
            parentChanged: true);

        var result = PostVersionMerger.Merge(
            oldVersion, newVersion, createHistory(oldVersion));

        Assert.Equal(
            new[] { "cat", "blue_hair" },
            result.AddedTags);

        Assert.Equal(
            new[] { "dog", "long_hair" },
            result.RemovedTags);

        Assert.Equal(PostRating.Explicit, result.NewRating);
        Assert.Equal("https://example.com/new", result.NewSource);
        Assert.True(result.SourceChanged);
        Assert.Equal(20, result.NewParent);
        Assert.True(result.ParentChanged);
    }

    [Fact]
    public void Merge_DoesNotMutateInputVersions()
    {
        var oldVersion = createVersion(
            addedTags: ["cat"],
            removedTags: ["dog"],
            newRating: PostRating.Sensitive);

        var newVersion = createVersion(
            addedTags: ["blue_hair"],
            removedTags: ["long_hair"],
            newRating: PostRating.Explicit);

        var oldAddedTags = oldVersion.AddedTags!.ToArray();
        var oldRemovedTags = oldVersion.RemovedTags!.ToArray();
        var newAddedTags = newVersion.AddedTags!.ToArray();
        var newRemovedTags = newVersion.RemovedTags!.ToArray();

        PostVersionMerger.Merge(
            oldVersion, newVersion, createHistory(oldVersion));

        Assert.Equal(oldAddedTags, oldVersion.AddedTags);
        Assert.Equal(oldRemovedTags, oldVersion.RemovedTags);
        Assert.Equal(newAddedTags, newVersion.AddedTags);
        Assert.Equal(newRemovedTags, newVersion.RemovedTags);
        Assert.Equal(PostRating.Sensitive, oldVersion.NewRating);
        Assert.Equal(PostRating.Explicit, newVersion.NewRating);
    }
}
