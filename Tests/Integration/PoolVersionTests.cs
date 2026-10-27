using CRbooru.Data;
using CRbooru.Models;
using CRbooru.Services;

namespace CRbooru.Tests.Integration;

public class PoolVersionTests
    : DatabaseLifetime,
      IClassFixture<CustomWebApplicationFactory<Program>>
{
    public PoolVersionTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateAsync_CreatesInitialVersion()
    {
        using var scope = CreateScope();
        var db = GetService<CRbooruContext>(scope);
        var pools = GetService<PoolService>(scope);
        var poolVersions = GetService<PoolVersionService>(scope);
        var users = GetService<UserService>(scope);
        var posts = GetService<PostService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Member);
        await users.Add(user);

        var post = ModelCreator.CreatePost(user, PostRating.General, null, null);
        db.Posts.Add(post);
        await db.SaveChangesAsync();

        var pool = await pools.CreateAsync(user, new PoolFormModel {
            Name = "new pool",
            Description = "Lorem ipsum",
            PostIds = "1"
        });

        var version = poolVersions.List(pool.Id, null)[0];
        Assert.Equal([1], version.NewState);
        Assert.Equal("new pool", version.NewName);
        Assert.Equal("Lorem ipsum", version.NewDescription);
        Assert.True(version.DescriptionChanged);
    }

    [Fact]
    public async Task UpdateAsync_OnlyPosts_CreatesCorrectVersion()
    {
        using var scope = CreateScope();
        var db = GetService<CRbooruContext>(scope);
        var pools = GetService<PoolService>(scope);
        var poolVersions = GetService<PoolVersionService>(scope);
        var users = GetService<UserService>(scope);
        var posts = GetService<PostService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Member);
        await users.Add(user);

        var post = ModelCreator.CreatePost(user, PostRating.General, null, null);
        db.Posts.Add(post);
        await db.SaveChangesAsync();

        var pool = await pools.CreateAsync(user, new PoolFormModel {
            Name = "new pool",
            Description = "Lorem ipsum",
            PostIds = "1"
        });

        var initialVersion = poolVersions.List(pool.Id, null)[0];
        initialVersion.CreatedAt = new DateTime(2025, 10, 26, 12, 0, 0);;
        await db.SaveChangesAsync();

        await pools.UpdateAsync(user, pool, new PoolFormModel {
            Name = "new pool",
            Description = "Lorem ipsum",
            PostIds = ""
        });

        var newVersion = poolVersions.List(pool.Id, null, true)[1];

        Assert.Null(newVersion.NewName);
        Assert.Null(newVersion.NewDescription);
        Assert.False(newVersion.DescriptionChanged);
        Assert.Equal(new List<int> {}, newVersion.NewState);
    }

    [Fact]
    public async Task UpdateAsync_NameAndDescription_CreatesCorrectVersion()
    {
        using var scope = CreateScope();
        var db = GetService<CRbooruContext>(scope);
        var pools = GetService<PoolService>(scope);
        var poolVersions = GetService<PoolVersionService>(scope);
        var users = GetService<UserService>(scope);
        var posts = GetService<PostService>(scope);

        var user = ModelCreator.CreateUser("NewUser", UserRole.Member);
        await users.Add(user);

        var post = ModelCreator.CreatePost(user, PostRating.General, null, null);
        db.Posts.Add(post);
        await db.SaveChangesAsync();

        var pool = await pools.CreateAsync(user, new PoolFormModel {
            Name = "new pool",
            Description = "Lorem ipsum",
            PostIds = "1"
        });

        var initialVersion = poolVersions.List(pool.Id, null)[0];
        initialVersion.CreatedAt = new DateTime(2025, 10, 26, 12, 0, 0);;
        await db.SaveChangesAsync();

        await pools.UpdateAsync(user, pool, new PoolFormModel {
            Name = "new pool name",
            Description = "lorem",
            PostIds = "1"
        });

        var newVersion = poolVersions.List(pool.Id, null, true)[1];

        Assert.Equal("new pool name", newVersion.NewName);
        Assert.Equal("lorem", newVersion.NewDescription);
        Assert.True(newVersion.DescriptionChanged);
        Assert.Null(newVersion.NewState);
    }
}
