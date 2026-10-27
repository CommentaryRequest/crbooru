using CRbooru.Data;
using CRbooru.Models;
using CRbooru.Services;
using CRbooru.Logical;

namespace CRbooru.Tests.Unit.Services;

public class PostTests
    : DatabaseLifetime,
      IClassFixture<CustomWebApplicationFactory<Program>>
{
    public PostTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateAsync_ValidData_CreatesCorrectPost()
    {
        using var scope = CreateScope();
        var mediaAssets = GetService<MediaAssetService>(scope);
        var posts = GetService<PostService>(scope);
        var users = GetService<UserService>(scope);

        var asset = ModelCreator.CreateDummyMediaAsset();
        await mediaAssets.AddAsync(asset);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Member);
        await users.Add(user);

        var formModel = new PostFormModel {
            MediaAssetId = asset.Id,
            Source = "https://example.com",
            Rating = PostRating.General,
            TagString = "art:bkub copy:touhou char:cirno 1girl solo meta:highres"
        };

        var post = await posts.CreateAsync(user, formModel);

        Assert.Equal(user.Id, post.Uploader.Id);
        Assert.Equal(asset.Id, post.MediaAsset.Id);
        Assert.Equal(PostRating.General, post.Rating);
        Assert.Equal("https://example.com", post.Source);
        Assert.Null(post.ParentId);
    }

    [Fact]
    public async Task CreateAsync_MissingMediaAsset_Throws()
    {
        using var scope = CreateScope();
        var posts = GetService<PostService>(scope);
        var users = GetService<UserService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Member);
        await users.Add(user);

        var formModel = new PostFormModel {
            MediaAssetId = 1,
            Rating = PostRating.General,
            TagString = "1girl"
        };

        await Assert.ThrowsAsync<ArgumentException>(async () => await posts.CreateAsync(user, formModel));
    }

    [Fact]
    public async Task CreateAsync_InvalidParent_Throws()
    {
        using var scope = CreateScope();
        var posts = GetService<PostService>(scope);
        var users = GetService<UserService>(scope);
        var mediaAssets = GetService<MediaAssetService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Member);
        await users.Add(user);

        var asset = ModelCreator.CreateDummyMediaAsset();
        await mediaAssets.AddAsync(asset);

        var formModel = new PostFormModel {
            MediaAssetId = asset.Id,
            Rating = PostRating.General,
            TagString = "1girl",
            Parent = 20
        };

        await Assert.ThrowsAsync<ArgumentException>(async () => await posts.CreateAsync(user, formModel));
    }

    [Fact]
    public async Task EditAsync_UpdateTagsParentAndSource_UpdatePostAndCreatesVersion()
    {
        using var scope = CreateScope();
        var db = GetService<CRbooruContext>(scope);
        var posts = GetService<PostService>(scope);
        var users = GetService<UserService>(scope);
        var mediaAssets = GetService<MediaAssetService>(scope);
        var postVersions = GetService<PostVersionService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Member);
        await users.Add(user);

        var asset1 = ModelCreator.CreateDummyMediaAsset();
        await mediaAssets.AddAsync(asset1);

        var asset2 = ModelCreator.CreateDummyMediaAsset();
        await mediaAssets.AddAsync(asset2);

        var createForm = new PostFormModel {
            MediaAssetId = asset1.Id,
            Rating = PostRating.General,
            TagString = "copy:touhou"
        };
        var post1 = await posts.CreateAsync(user, createForm);

        createForm = new PostFormModel {
            MediaAssetId = asset2.Id,
            Rating = PostRating.Sensitive,
            TagString = "copy:original",
            Parent = post1.Id
        };
        var post2 = await posts.CreateAsync(user, createForm);

        // Offset the 2nd post's initial version back by like a year
        // so that it definitely doesn't merge
        var postVersion = postVersions.List(post2.Id, null);
        postVersion[0].CreatedAt = new DateTime(2025, 10, 26, 12, 0, 0);
        await db.SaveChangesAsync();

        var editForm = new PostFormModel {
            Rating = PostRating.Questionable,
            TagString = "touhou 1girl solo",
            Parent = null,
            Source = "https://example.com"
        };
        await posts.EditAsync(user, post2, editForm);

        // check for post's new state
        Assert.Equal(PostRating.Questionable, post2.Rating);
        Assert.Equal("touhou \n1girl solo \n", TagParser.BuildTagString(post2.Tags));
        Assert.Null(post2.ParentId);
        Assert.Equal("https://example.com", post2.Source);

        // check new post version
        var newVersion = postVersions.List(post2.Id, null, true)[1];
        Assert.NotNull(newVersion.AddedTags);
        Assert.Equal(["touhou", "1girl", "solo"], newVersion.AddedTags);
        Assert.NotNull(newVersion.RemovedTags);
        Assert.Equal(["original"], newVersion.RemovedTags);
        Assert.NotNull(newVersion.NewRating);
        Assert.Equal(PostRating.Questionable, newVersion.NewRating);
        Assert.True(newVersion.SourceChanged);
        Assert.NotNull(newVersion.NewSource);
        Assert.Equal("https://example.com", newVersion.NewSource);
        Assert.True(newVersion.ParentChanged);
        Assert.Null(newVersion.NewParent);
    }

    [Fact]
    public async Task EditAsync_AddMetaTags_Works()
    {
        using var scope = CreateScope();
        var db = GetService<CRbooruContext>(scope);
        var posts = GetService<PostService>(scope);
        var users = GetService<UserService>(scope);
        var mediaAssets = GetService<MediaAssetService>(scope);
        var postVersions = GetService<PostVersionService>(scope);
        var pools = GetService<PoolService>(scope);
        var poolPosts = GetService<PoolPostService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Member);
        await users.Add(user);

        var poolFormModel = new PoolFormModel {
            Name = "a",
            Description = "",
            PostIds = ""
        };
        var pool = await pools.CreateAsync(user, poolFormModel);

        var asset1 = ModelCreator.CreateDummyMediaAsset();
        await mediaAssets.AddAsync(asset1);

        var asset2 = ModelCreator.CreateDummyMediaAsset();
        await mediaAssets.AddAsync(asset2);

        var createForm = new PostFormModel {
            MediaAssetId = asset1.Id,
            Rating = PostRating.General,
            TagString = "copy:touhou"
        };
        var post1 = await posts.CreateAsync(user, createForm);

        createForm = new PostFormModel {
            MediaAssetId = asset2.Id,
            Rating = PostRating.General,
            TagString = "copy:original",
            Parent = post1.Id
        };
        var post2 = await posts.CreateAsync(user, createForm);

        var editForm = new PostFormModel {
            Rating = PostRating.General,
            TagString = "parent:none rating:e source:source123 pool:1"
        };
        await posts.EditAsync(user, post2, editForm);

        var postPools = await poolPosts.GetPostPools(post2.Id);

        Assert.Null(post2.ParentId);
        Assert.Equal(PostRating.Explicit, post2.Rating);
        Assert.Equal("source123", post2.Source);
        Assert.Equal(pool, postPools[0]);
    }
}
