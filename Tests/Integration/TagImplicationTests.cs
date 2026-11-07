using CRbooru.Models;
using CRbooru.Services;
using CRbooru.Data;

namespace CRbooru.Tests.Integration;

public class TagImplicationTests
    : DatabaseLifetime,
      IClassFixture<CustomWebApplicationFactory<Program>>
{
    public TagImplicationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task AddTags_WithImplications_AddsImpliedTags()
    {
        using var scope = CreateScope();
        var tagService = GetService<TagService>(scope);
        var posts = GetService<PostService>(scope);
        var postUpdate = GetService<PostUpdateService>(scope);
        var tagImplications = GetService<TagImplicationService>(scope);

        var user = ModelCreator.CreateUser("test", UserRole.Member);
        var tags = await tagService.ResolveTags(["1girl", "solo"]);
        await tagImplications.AddAsync(tags.ElementAt(1), tags.ElementAt(0)); // imply solo -> 1girl
        var post = ModelCreator.CreatePost(user, PostRating.General, null, null);

        await postUpdate.UpdateTags(post, ["solo"], []);
        Assert.Contains(tags.ElementAt(0), post.Tags);
        Assert.Contains(tags.ElementAt(1), post.Tags);
    }

    [Fact]
    public async Task RemoveTags_ImpliedTags_PreventRemoval()
    {
        using var scope = CreateScope();
        var tagService = GetService<TagService>(scope);
        var postUpdate = GetService<PostUpdateService>(scope);
        var tagImplications = GetService<TagImplicationService>(scope);

        var user = ModelCreator.CreateUser("test", UserRole.Member);
        var tags = await tagService.ResolveTags(["1girl", "solo", "blonde_hair"]);
        await tagImplications.AddAsync(tags.ElementAt(1), tags.ElementAt(0)); // imply solo -> 1girl
        await tagImplications.AddAsync(tags.ElementAt(2), tags.ElementAt(1)); // imply blonde_hair -> solo
        var post = ModelCreator.CreatePost(user, PostRating.General, null, null);

        await postUpdate.UpdateTags(post, ["blonde_hair"], []);
        await postUpdate.UpdateTags(post, [], ["1girl"]);
        await postUpdate.UpdateTags(post, [], ["solo"]);

        Assert.Contains(tags.ElementAt(0), post.Tags);
        Assert.Contains(tags.ElementAt(1), post.Tags);
        Assert.Contains(tags.ElementAt(2), post.Tags);

        await postUpdate.UpdateTags(post, [], ["blonde_hair", "solo"]);

        Assert.Contains(tags.ElementAt(0), post.Tags);
        Assert.DoesNotContain(tags.ElementAt(1), post.Tags);
        Assert.DoesNotContain(tags.ElementAt(2), post.Tags);
    }

    [Fact]
    public async Task AddTags_ImplyAfterCreatePost_AddImpliedTags()
    {
        using var scope = CreateScope();
        var db = GetService<CRbooruContext>(scope);
        var tagService = GetService<TagService>(scope);
        var postUpdate = GetService<PostUpdateService>(scope);
        var tagImplications = GetService<TagImplicationService>(scope);
        var posts = GetService<PostService>(scope);

        await CreateSystemUser(scope);
        var user = ModelCreator.CreateUser("test", UserRole.Member);
        var tags = await tagService.ResolveTags(["1girl", "solo", "blonde_hair"]);
        var post = ModelCreator.CreatePost(user, PostRating.General, null, null);
        db.Posts.Add(post);
        await postUpdate.UpdateTags(post, ["solo"], []);

        var implication1 = await tagImplications.AddAsync(tags.ElementAt(1), tags.ElementAt(0)); // imply solo -> 1girl
        var implication2 = await tagImplications.AddAsync(tags.ElementAt(0), tags.ElementAt(2)); // imply 1girl -> blonde_hair
        await posts.UpdateImplicationsAsync(implication1);
        await posts.UpdateImplicationsAsync(implication2);

        Assert.Contains(tags.ElementAt(0), post.Tags);
        Assert.Contains(tags.ElementAt(2), post.Tags);
    }
}
