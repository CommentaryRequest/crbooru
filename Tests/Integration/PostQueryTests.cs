using CRbooru.Data;
using CRbooru.Models;
using CRbooru.Services;
using CRbooru.Logical;
using Microsoft.EntityFrameworkCore;

namespace CRbooru.Tests.Integration;

public class PostQueryTests
    : DatabaseLifetime,
      IClassFixture<CustomWebApplicationFactory<Program>>
{
    public PostQueryTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private async Task<User> CreateUserAsync(UserService users, string name)
    {
        var user = ModelCreator.CreateUser(name, UserRole.Member);
        await users.Add(user);
        return user;
    }

    private async Task<Post> CreatePostAsync(
        PostService posts,
        MediaAssetService mediaAssets,
        User uploader,
        string tags = "1girl",
        PostRating rating = PostRating.General,
        string? source = null,
        int? parent = null)
    {
        var asset = ModelCreator.CreateDummyMediaAsset();
        await mediaAssets.AddAsync(asset);

        return await posts.CreateAsync(uploader, new PostFormModel {
            MediaAssetId = asset.Id,
            Rating = rating,
            Source = source,
            TagString = tags,
            Parent = parent
        });
    }

    private static async Task<(List<int> Ids, int Limit)> SearchAsync(
        PostService posts,
        params SearchTerm[] terms)
    {
        var (query, limit) = await posts.QuerySearchAsync(terms.ToList());
        var ids = await query.Select(p => p.Id).ToListAsync();

        return (ids, limit);
    }

    [Fact]
    public async Task QuerySearchAsync_NoTerms_ReturnsAllPostsInDescendingIdOrder()
    {
        using var scope = CreateScope();
        var posts = GetService<PostService>(scope);
        var users = GetService<UserService>(scope);
        var mediaAssets = GetService<MediaAssetService>(scope);

        var user = await CreateUserAsync(users, "SearchUser");

        var post1 = await CreatePostAsync(posts, mediaAssets, user);
        var post2 = await CreatePostAsync(posts, mediaAssets, user);
        var post3 = await CreatePostAsync(posts, mediaAssets, user);

        var (ids, limit) = await SearchAsync(posts);

        Assert.Equal([post3.Id, post2.Id, post1.Id], ids);
        Assert.True(limit > 0);
    }

    [Fact]
    public async Task QuerySearchAsync_TagFilters_MatchesAndNegatesTags()
    {
        using var scope = CreateScope();
        var posts = GetService<PostService>(scope);
        var users = GetService<UserService>(scope);
        var mediaAssets = GetService<MediaAssetService>(scope);

        var user = await CreateUserAsync(users, "SearchUser");

        var matching = await CreatePostAsync(
            posts, mediaAssets, user, "touhou cirno 1girl");
        await CreatePostAsync(posts, mediaAssets, user, "touhou reimu 1girl");
        var excluded = await CreatePostAsync(
            posts, mediaAssets, user, "vocaloid miku 1girl");

        var (ids, _) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.Tag, "touhou", false),
            new SearchTerm(SearchTermType.Tag, "cirno", false));

        Assert.Equal([matching.Id], ids);

        (ids, _) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.Tag, "touhou", true),
            new SearchTerm(SearchTermType.Tag, "cirno", false));

        Assert.Empty(ids);

        (ids, _) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.Tag, "touhou", true));

        Assert.Contains(excluded.Id, ids);
        Assert.DoesNotContain(matching.Id, ids);
    }

    [Fact]
    public async Task QuerySearchAsync_RatingFilters_MatchesAndNegatesRating()
    {
        using var scope = CreateScope();
        var posts = GetService<PostService>(scope);
        var users = GetService<UserService>(scope);
        var mediaAssets = GetService<MediaAssetService>(scope);

        var user = await CreateUserAsync(users, "SearchUser");

        var general = await CreatePostAsync(
            posts, mediaAssets, user, rating: PostRating.General);
        var explicitPost = await CreatePostAsync(
            posts, mediaAssets, user, rating: PostRating.Explicit);

        var (ids, _) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.Rating, "g", false));

        Assert.Equal([general.Id], ids);

        (ids, _) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.Rating, "g", true));

        Assert.Contains(explicitPost.Id, ids);
        Assert.DoesNotContain(general.Id, ids);
    }

    [Fact]
    public async Task QuerySearchAsync_IdFilters_MatchesExactIdAndNegatesIt()
    {
        using var scope = CreateScope();
        var posts = GetService<PostService>(scope);
        var users = GetService<UserService>(scope);
        var mediaAssets = GetService<MediaAssetService>(scope);

        var user = await CreateUserAsync(users, "SearchUser");

        var post1 = await CreatePostAsync(posts, mediaAssets, user);
        var post2 = await CreatePostAsync(posts, mediaAssets, user);
        var post3 = await CreatePostAsync(posts, mediaAssets, user);

        var (ids, _) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.Id, post2.Id.ToString(), false));

        Assert.Equal([post2.Id], ids);

        (ids, _) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.Id, post2.Id.ToString(), true));

        Assert.DoesNotContain(post2.Id, ids);
        Assert.Contains(post1.Id, ids);
        Assert.Contains(post3.Id, ids);
    }

    [Fact]
    public async Task QuerySearchAsync_IdRange_MatchesInclusiveBounds()
    {
        using var scope = CreateScope();
        var posts = GetService<PostService>(scope);
        var users = GetService<UserService>(scope);
        var mediaAssets = GetService<MediaAssetService>(scope);

        var user = await CreateUserAsync(users, "SearchUser");

        var post1 = await CreatePostAsync(posts, mediaAssets, user);
        var post2 = await CreatePostAsync(posts, mediaAssets, user);
        var post3 = await CreatePostAsync(posts, mediaAssets, user);
        var post4 = await CreatePostAsync(posts, mediaAssets, user);

        var range = $"{post2.Id}..{post3.Id}";

        var (ids, _) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.Id, range, false));

        Assert.Equal([post3.Id, post2.Id], ids);

        (ids, _) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.Id, range, true));

        Assert.DoesNotContain(post2.Id, ids);
        Assert.DoesNotContain(post3.Id, ids);
        Assert.Contains(post1.Id, ids);
        Assert.Contains(post4.Id, ids);
    }

    [Fact]
    public async Task QuerySearchAsync_UserFiltersByUploaderName()
    {
        using var scope = CreateScope();
        var posts = GetService<PostService>(scope);
        var users = GetService<UserService>(scope);
        var mediaAssets = GetService<MediaAssetService>(scope);

        var user1 = await CreateUserAsync(users, "SearchUser1");
        var user2 = await CreateUserAsync(users, "SearchUser2");

        var post1 = await CreatePostAsync(posts, mediaAssets, user1);
        await CreatePostAsync(posts, mediaAssets, user2);

        var (ids, _) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.User, user1.Name, false));

        Assert.Equal([post1.Id], ids);

        (ids, _) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.User, user1.Name, true));

        Assert.DoesNotContain(post1.Id, ids);
    }

    [Fact]
    public async Task QuerySearchAsync_LimitChangesReturnedLimitWithoutFilteringPosts()
    {
        using var scope = CreateScope();
        var posts = GetService<PostService>(scope);
        var users = GetService<UserService>(scope);
        var mediaAssets = GetService<MediaAssetService>(scope);

        var user = await CreateUserAsync(users, "SearchUser");

        var post1 = await CreatePostAsync(posts, mediaAssets, user);
        var post2 = await CreatePostAsync(posts, mediaAssets, user);

        var (ids, limit) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.Limit, "1", false));

        Assert.Equal(1, limit);
        Assert.Equal([post2.Id, post1.Id], ids);
    }

    [Fact]
    public async Task QuerySearchAsync_ParentFilters_MatchesParentAndChildren()
    {
        using var scope = CreateScope();
        var posts = GetService<PostService>(scope);
        var users = GetService<UserService>(scope);
        var mediaAssets = GetService<MediaAssetService>(scope);

        var user = await CreateUserAsync(users, "SearchUser");

        var parent = await CreatePostAsync(posts, mediaAssets, user);
        var child = await CreatePostAsync(
            posts, mediaAssets, user, parent: parent.Id);
        var unrelated = await CreatePostAsync(posts, mediaAssets, user);

        var (ids, _) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.Parent, parent.Id.ToString(), false));

        Assert.Contains(parent.Id, ids);
        Assert.Contains(child.Id, ids);
        Assert.DoesNotContain(unrelated.Id, ids);

        (ids, _) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.Parent, parent.Id.ToString(), true));

        Assert.DoesNotContain(parent.Id, ids);
        Assert.DoesNotContain(child.Id, ids);
        Assert.Contains(unrelated.Id, ids);
    }

    [Fact]
    public async Task QuerySearchAsync_ParentNoneAndAny_FilterCorrectly()
    {
        using var scope = CreateScope();
        var posts = GetService<PostService>(scope);
        var users = GetService<UserService>(scope);
        var mediaAssets = GetService<MediaAssetService>(scope);

        var user = await CreateUserAsync(users, "SearchUser");

        var parent = await CreatePostAsync(posts, mediaAssets, user);
        var child = await CreatePostAsync(
            posts, mediaAssets, user, parent: parent.Id);

        var (ids, _) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.Parent, "none", false));

        Assert.Contains(parent.Id, ids);
        Assert.DoesNotContain(child.Id, ids);

        (ids, _) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.Parent, "any", false));

        Assert.Equal([child.Id], ids);
    }

    [Fact]
    public async Task QuerySearchAsync_ChildNoneAndAny_FilterCorrectly()
    {
        using var scope = CreateScope();
        var posts = GetService<PostService>(scope);
        var users = GetService<UserService>(scope);
        var mediaAssets = GetService<MediaAssetService>(scope);

        var user = await CreateUserAsync(users, "SearchUser");

        var parent = await CreatePostAsync(posts, mediaAssets, user);
        var child = await CreatePostAsync(
            posts, mediaAssets, user, parent: parent.Id);

        var (ids, _) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.Child, "any", false));

        Assert.Equal([parent.Id], ids);

        (ids, _) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.Child, "none", false));

        Assert.Equal([child.Id], ids);
    }

    [Fact]
    public async Task QuerySearchAsync_InvalidRatingAndId_AreIgnored()
    {
        using var scope = CreateScope();
        var posts = GetService<PostService>(scope);
        var users = GetService<UserService>(scope);
        var mediaAssets = GetService<MediaAssetService>(scope);

        var user = await CreateUserAsync(users, "SearchUser");

        var post1 = await CreatePostAsync(posts, mediaAssets, user);
        var post2 = await CreatePostAsync(posts, mediaAssets, user);

        var (ids, _) = await SearchAsync(
            posts,
            new SearchTerm(SearchTermType.Rating, "not-a-rating", false),
            new SearchTerm(SearchTermType.Id, "not-an-id", false));

        Assert.Equal([post2.Id, post1.Id], ids);
    }
}
