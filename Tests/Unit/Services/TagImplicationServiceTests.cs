using CRbooru.Models;
using CRbooru.Services;

namespace CRbooru.Tests.Unit.Services;

public class TagImplicationServiceTests
    : DatabaseLifetime,
      IClassFixture<CustomWebApplicationFactory<Program>>
{
    public TagImplicationServiceTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task AddAsync_ValidImplication_AddsImplicationToDatabase()
    {
        using var scope = CreateScope();
        var tagService = GetService<TagService>(scope);
        var tagImplications = GetService<TagImplicationService>(scope);

        var tags = await tagService.ResolveTags(["a", "b"]);

        var implication = await tagImplications.AddAsync(tags.ElementAt(0), tags.ElementAt(1));
        Assert.Equal(tags.ElementAt(0).Id, implication.Antecedent.Id);
        Assert.Equal(tags.ElementAt(1).Id, implication.Consequent.Id);
    }

    [Fact]
    public async Task AddAsync_SameTag_Throws()
    {
        using var scope = CreateScope();
        var tagService = GetService<TagService>(scope);
        var tagImplications = GetService<TagImplicationService>(scope);

        var tag = (await tagService.ResolveTags(["a"])).ElementAt(0);

        await Assert.ThrowsAsync<TagImplicationException>(async () => await tagImplications.AddAsync(tag, tag));
    }

    [Fact]
    public async Task AddAsync_DifferentCategory_Throws()
    {
        using var scope = CreateScope();
        var tagService = GetService<TagService>(scope);
        var tagImplications = GetService<TagImplicationService>(scope);

        var tags = await tagService.ResolveTags(["char:touhou", "copy:bkub"]);

        await Assert.ThrowsAsync<TagImplicationException>(async () => await tagImplications.AddAsync(tags.ElementAt(0), tags.ElementAt(1)));
    }
}
