using Microsoft.AspNetCore.Http;
using CRbooru.Services;
using CRbooru.Data;
using CRbooru.Models;

namespace CRbooru.Tests.Unit.Services;

public class MediaAssetServiceTests
    : DatabaseLifetime,
      IClassFixture<CustomWebApplicationFactory<Program>>
{
    public MediaAssetServiceTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private static IFormFile createFormFile(string path)
    {
        var stream = File.OpenRead(path);
        return new FormFile(stream, baseStreamOffset: 0, length: stream.Length, name: "file", fileName: Path.GetFileName(path));
    }

    [Fact]
    public async Task CreateFromFile_ValidFile_CreatesAssetAndProcessesFile()
    {
        using var tempDir = new TemporaryDirectory();
        using var scope = CreateScope();

        // Creating a new service manually to pass it a temporary
        // images root path.
        var db = GetService<CRbooruContext>(scope);

        var storageOptions = new StorageOptions(tempDir.GetPath("images"));
        var service = new MediaAssetService(db, storageOptions);

        var formFile = createFormFile(FileFixture.GetFixturePath("image.jpg"));
        var asset = await service.CreateFromFile(formFile);

        Assert.Equal(960, asset.Width);
        Assert.Equal(762, asset.Height);
        Assert.Equal("c194b6b065aecef7efdd118e61218755", asset.Md5);
        Assert.Equal("jpg", asset.FileType);
        Assert.Equal(formFile.Length, asset.FileSize);
        Assert.True(File.Exists(asset.GetFilePath(storageOptions.ImagesPath, MediaAssetVariantType.Original)));
        Assert.True(File.Exists(asset.GetFilePath(storageOptions.ImagesPath, MediaAssetVariantType.Sample)));
        Assert.True(File.Exists(asset.GetFilePath(storageOptions.ImagesPath, MediaAssetVariantType.Thumbnail)));
    }

    [Fact]
    public async Task Get_ValidId_ReturnsCorrectAsset()
    {
        using var scope = CreateScope();
        var service = GetService<MediaAssetService>(scope);
        var db = GetService<CRbooruContext>(scope);

        var asset = ModelCreator.CreateDummyMediaAsset();
        db.MediaAssets.Add(asset);
        await db.SaveChangesAsync();

        var dbAsset = await service.Get(1);
        Assert.NotNull(dbAsset);
    }

    [Fact]
    public async Task Get_InvalidId_ReturnsNull()
    {
        using var scope = CreateScope();
        var service = GetService<MediaAssetService>(scope);
        var db = GetService<CRbooruContext>(scope);

        var asset = ModelCreator.CreateDummyMediaAsset();
        db.MediaAssets.Add(asset);
        await db.SaveChangesAsync();

        var dbAsset = await service.Get(20);
        Assert.Null(dbAsset);
    }

    [Fact]
    public async Task ByMd5_ValidMd5_ReturnsCorrectAsset()
    {
        using var scope = CreateScope();
        var service = GetService<MediaAssetService>(scope);
        var db = GetService<CRbooruContext>(scope);

        var asset = ModelCreator.CreateDummyMediaAsset("44444444444444444444444444444444"); // unfair dice roll
        db.MediaAssets.Add(asset);
        await db.SaveChangesAsync();

        var dbAsset = await service.ByMd5("44444444444444444444444444444444");
        Assert.NotNull(dbAsset);
    }

    [Fact]
    public async Task ByMd5_InvalidMd5_ReturnsNull()
    {
        using var scope = CreateScope();
        var service = GetService<MediaAssetService>(scope);
        var db = GetService<CRbooruContext>(scope);

        var asset = ModelCreator.CreateDummyMediaAsset("44444444444444444444444444444444");
        db.MediaAssets.Add(asset);
        await db.SaveChangesAsync();

        var dbAsset = await service.ByMd5("cdefabcdefabcdefabcdefabcdefabcd");
        Assert.Null(dbAsset);
    }
}
