using CRbooru.Data;
using CRbooru.Models;
using CRbooru.Services;

namespace CRbooru.Tests.Integration;

public class PoolTests
    : DatabaseLifetime,
      IClassFixture<CustomWebApplicationFactory<Program>>
{
    public PoolServiceTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateAsync_Works()
    {
        using var scope = CreateScope();
        var db = GetService<CRbooruContext>(scope);
        var pools = GetService<PoolService>(scope);
        var users = GetService<UserService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Member);
        await users.Add(user);

        for (var i = 0; i < 4; i++) {
            var post = ModelCreator.CreatePost(user, PostRating.General, null, null);
            db.Posts.Add(post);
        }
        await db.SaveChangesAsync();

        var formModel = new PoolFormModel {
            Name = "new pool",
            Description = "Lorem ipsum",
            PostIds = "1 3 4 2"
        };
        var pool = await pools.CreateAsync(user, formModel);

        Assert.Equal("new pool", pool.Name);
        Assert.Equal("Lorem ipsum", pool.Description);
        Assert.Equal(new List<int> {1, 3, 4, 2}, pool.PostIds);
    }

    [Fact]
    public async Task GetPostsAsync_Works()
    {
        using var scope = CreateScope();
        var db = GetService<CRbooruContext>(scope);
        var pools = GetService<PoolService>(scope);
        var users = GetService<UserService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Member);
        await users.Add(user);

        var posts = new List<Post> {};

        for (var i = 0; i < 4; i++) {
            var post = ModelCreator.CreatePost(user, PostRating.General, null, null);
            db.Posts.Add(post);
            posts.Add(post);
        }
        await db.SaveChangesAsync();

        var formModel = new PoolFormModel {
            Name = "new pool",
            Description = "Lorem ipsum",
            PostIds = "1 3 4 2"
        };
        var pool = await pools.CreateAsync(user, formModel);
        var poolPosts = await pools.GetPostsAsync(pool);

        Assert.Equal(posts[0], poolPosts[0]);
        Assert.Equal(posts[2], poolPosts[1]);
        Assert.Equal(posts[3], poolPosts[2]);
        Assert.Equal(posts[1], poolPosts[3]);
    }

    [Fact]
    public async Task UpdateAsync_AddAndRemove_Works()
    {
        using var scope = CreateScope();
        var db = GetService<CRbooruContext>(scope);
        var pools = GetService<PoolService>(scope);
        var users = GetService<UserService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Member);
        await users.Add(user);

        var posts = new List<Post> {};

        for (var i = 0; i < 4; i++) {
            var post = ModelCreator.CreatePost(user, PostRating.General, null, null);
            db.Posts.Add(post);
            posts.Add(post);
        }
        await db.SaveChangesAsync();

        var formModel = new PoolFormModel {
            Name = "new pool",
            Description = "Lorem ipsum",
            PostIds = "1 3 2"
        };
        var pool = await pools.CreateAsync(user, formModel);

        var updateFormModel = new PoolFormModel {
            Name = "new pool",
            Description = "Lorem ipsum",
            PostIds = "4 1 3"
        };
        await pools.UpdateAsync(user, pool, updateFormModel);

        var poolPosts = await pools.GetPostsAsync(pool);
        Assert.Equal(posts[3], poolPosts[0]);
        Assert.Equal(posts[0], poolPosts[1]);
        Assert.Equal(posts[2], poolPosts[2]);
    }

    [Fact]
    public async Task UpdateAsync_AddInvalidIds_IgnoreInvalidIds()
    {
        using var scope = CreateScope();
        var db = GetService<CRbooruContext>(scope);
        var pools = GetService<PoolService>(scope);
        var users = GetService<UserService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Member);
        await users.Add(user);

        var posts = new List<Post> {};

        for (var i = 0; i < 4; i++) {
            var post = ModelCreator.CreatePost(user, PostRating.General, null, null);
            db.Posts.Add(post);
            posts.Add(post);
        }
        await db.SaveChangesAsync();

        var formModel = new PoolFormModel {
            Name = "new pool",
            Description = "Lorem ipsum",
            PostIds = "1 3 2"
        };
        var pool = await pools.CreateAsync(user, formModel);

        var updateFormModel = new PoolFormModel {
            Name = "new pool",
            Description = "Lorem ipsum",
            PostIds = "1 3 2 test"
        };
        await pools.UpdateAsync(user, pool, updateFormModel);

        var poolPosts = await pools.GetPostsAsync(pool);
        Assert.Equal(posts[0], poolPosts[0]);
        Assert.Equal(posts[2], poolPosts[1]);
        Assert.Equal(posts[1], poolPosts[2]);
    }

    [Fact]
    public async Task UpdateAsync_UpdateNameAndDescription_Works()
    {
        using var scope = CreateScope();
        var db = GetService<CRbooruContext>(scope);
        var pools = GetService<PoolService>(scope);
        var users = GetService<UserService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Member);
        await users.Add(user);

        var formModel = new PoolFormModel {
            Name = "new pool",
            Description = "Lorem ipsum",
            PostIds = ""
        };
        var pool = await pools.CreateAsync(user, formModel);

        var updateFormModel = new PoolFormModel {
            Name = "test pool",
            Description = "Only distinguished, well-drawn, nicely-shaped feet.",
            PostIds = ""
        };
        await pools.UpdateAsync(user, pool, updateFormModel);

        Assert.Equal("test pool", pool.Name);
        Assert.Equal("Only distinguished, well-drawn, nicely-shaped feet.", pool.Description);
    }

    [Fact]
    public async Task UpdateAsync_EmptyName_Throws()
    {
        using var scope = CreateScope();
        var db = GetService<CRbooruContext>(scope);
        var pools = GetService<PoolService>(scope);
        var users = GetService<UserService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Member);
        await users.Add(user);

        var formModel = new PoolFormModel {
            Name = "new pool",
            Description = "Lorem ipsum",
            PostIds = ""
        };
        var pool = await pools.CreateAsync(user, formModel);

        var updateFormModel = new PoolFormModel {
            Name = "",
            Description = "",
            PostIds = ""
        };
        await Assert.ThrowsAsync<ArgumentException>(async () => await pools.UpdateAsync(user, pool, updateFormModel));
    }

    [Fact]
    public async Task GetPostPools_Works()
    {
        using var scope = CreateScope();
        var db = GetService<CRbooruContext>(scope);
        var pools = GetService<PoolService>(scope);
        var users = GetService<UserService>(scope);
        var poolPosts = GetService<PoolPostService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Member);
        await users.Add(user);

        var posts = new List<Post> {};

        for (var i = 0; i < 4; i++) {
            var post = ModelCreator.CreatePost(user, PostRating.General, null, null);
            db.Posts.Add(post);
            posts.Add(post);
        }
        await db.SaveChangesAsync();

        var formModel = new PoolFormModel {
            Name = "new pool",
            Description = "Lorem ipsum",
            PostIds = "1 2 3"
        };
        var pool1 = await pools.CreateAsync(user, formModel);

        formModel = new PoolFormModel {
            Name = "another pool",
            Description = "ipsum Lorem",
            PostIds = "2 3 4"
        };
        var pool2 = await pools.CreateAsync(user, formModel);

        Assert.Equal(new List<Pool> {pool1, pool2}, await poolPosts.GetPostPools(2));
        Assert.Equal(new List<Pool> {pool1}, await poolPosts.GetPostPools(1));
        Assert.Equal(new List<Pool> {pool2}, await poolPosts.GetPostPools(4));
    }
}
