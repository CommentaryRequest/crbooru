using CRbooru.Logical;
using CRbooru.Models;

namespace CRbooru.Tests.Unit;

public class PostVersionUndoerTests
{
    private readonly List<PostVersion> Versions = new List<PostVersion>() {
        new PostVersion(1, 1, ["1girl", "solo"], [], PostRating.Sensitive, "https://example.com", true, 2, true),
        new PostVersion(1, 1, [], [], PostRating.General, null, false, null, false),
        new PostVersion(1, 1, ["long_hair"], ["solo"], null, null, false, null, false),
        new PostVersion(1, 1, [], [], null, "new source", true, null, false),
        new PostVersion(1, 1, [], [], null, null, false, 3, true),
        new PostVersion(1, 1, ["green_hair", "yellow_eyes"], ["1girl", "long_hair"], PostRating.Explicit, "newest source", true, 10, true)
    };

    private List<PostVersion> createVersions(PostRating currentRating, string? currentSource, int? currentParent)
    {
        var user = ModelCreator.CreateUser("test", UserRole.Member);
        var post = ModelCreator.CreatePost(user, currentRating, currentSource, currentParent);

        var versions = Versions.Select(p => new PostVersion(p)).ToList();
        for (int i = 0; i < versions.Count; i++) {
            var version = versions[i];
            version.Post = post;
            version.Number = i + 1;
        }

        return versions;
    }

    [Fact]
    public void Undo_FirstVersion_ThrowException()
    {
        var versions = createVersions(PostRating.General, null, null);
        Assert.Throws<ArgumentException>(() => PostVersionUndoer.Undo(versions[0], versions));
    }

    [Fact]
    public void Undo_TagsOnly_UndoOnlyTags()
    {
        var versions = createVersions(PostRating.General, null, null);
        var version = PostVersionUndoer.Undo(versions[2], versions);

        Assert.NotNull(version.AddedTags);
        Assert.NotNull(version.RemovedTags);
        Assert.Equal(["solo"], version.AddedTags);
        Assert.Equal(["long_hair"], version.RemovedTags);
        Assert.Null(version.NewRating);
        Assert.False(version.SourceChanged);
        Assert.False(version.ParentChanged);
    }

    [Fact]
    public void Undo_RatingOnly_UndoOnlyRating()
    {
        var versions = createVersions(PostRating.General, null, null);
        var version = PostVersionUndoer.Undo(versions[1], versions);

        Assert.True(version.AddedTags == null || !version.AddedTags.Any());
        Assert.True(version.RemovedTags == null || !version.RemovedTags.Any());
        Assert.Equal(PostRating.Sensitive, version.NewRating);
        Assert.False(version.SourceChanged);
        Assert.False(version.ParentChanged);
    }

    [Fact]
    public void Undo_SourceOnly_UndoOnlySource()
    {
        var versions = createVersions(PostRating.General, "new source", null);
        var version = PostVersionUndoer.Undo(versions[3], versions);

        Assert.True(version.AddedTags == null || !version.AddedTags.Any());
        Assert.True(version.RemovedTags == null || !version.RemovedTags.Any());
        Assert.Null(version.NewRating);
        Assert.True(version.SourceChanged);
        Assert.Equal("https://example.com", version.NewSource);
        Assert.False(version.ParentChanged);
        Assert.Null(version.NewParent);
    }

    [Fact]
    public void Undo_ParentOnly_UndoOnlyParent()
    {
        var versions = createVersions(PostRating.General, null, 3);
        var version = PostVersionUndoer.Undo(versions[4], versions);

        Assert.True(version.AddedTags == null || !version.AddedTags.Any());
        Assert.True(version.RemovedTags == null || !version.RemovedTags.Any());
        Assert.Null(version.NewRating);
        Assert.False(version.SourceChanged);
        Assert.Null(version.NewSource);
        Assert.True(version.ParentChanged);
        Assert.NotNull(version.NewParent);
        Assert.Equal(2, version.NewParent);
    }

    [Fact]
    public void Undo_AllFields_UndoAllFields()
    {
        var versions = createVersions(PostRating.Explicit, "newest source", 10);
        var version = PostVersionUndoer.Undo(versions[5], versions);

        Assert.NotNull(version.AddedTags);
        Assert.Equal(["1girl", "long_hair"], version.AddedTags);
        Assert.NotNull(version.RemovedTags);
        Assert.Equal(["green_hair", "yellow_eyes"], version.RemovedTags);
        Assert.Equal(PostRating.General, version.NewRating);
        Assert.Equal("new source", version.NewSource);
        Assert.True(version.SourceChanged);
        Assert.Equal(3, version.NewParent);
        Assert.True(version.ParentChanged);
    }
}
