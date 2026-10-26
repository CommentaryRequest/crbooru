using NetVips;
using CRbooru.Logical;
using CRbooru.Models;

namespace CRbooru.Tests.Unit.Logical;

public class MediaFileProcessorTests
{
    // Image sources:
    // https://commons.wikimedia.org/wiki/File:Sunflower_Public_Domain.jpg
    // https://commons.wikimedia.org/wiki/File:Apple-tree_blossoms_2017_G3.jpg

    private static string getFixturePath(string filename)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", filename);
    }

    [Theory]
    [InlineData("image.jpg", "c194b6b065aecef7efdd118e61218755", "jpg")]
    [InlineData("image.png", "4a537a667671109b04b9938a7be5895c", "png")]
    [InlineData("image.webp", "8c1f50a68ee45f9f08a614f8c1f8ed7b", "webp")]
    [InlineData("image.avif", "307bb3b1c290027f0a8f9e7966626df5", "avif")]
    public void ProcessFile_ValidImage_ProducesExpectedAssetAndFiles(string input, string expectedMd5, string expectedExtension)
    {
        using var tempDir = new TemporaryDirectory();

        var fixturePath = getFixturePath(input);

        using var stream = File.OpenRead(fixturePath);
        var imagesRoot = tempDir.GetPath("images");
        var asset = MediaFileProcessor.ProcessFile(stream, imagesRoot);

        Assert.Equal(960, asset.Width);
        Assert.Equal(762, asset.Height);
        Assert.Equal(expectedMd5, asset.Md5);
        Assert.Equal(expectedExtension, asset.FileType);
        Assert.Equal(stream.Length, asset.FileSize);

        Assert.True(File.Exists(asset.GetFilePath(imagesRoot, MediaAssetVariantType.Original)));
        Assert.True(File.Exists(asset.GetFilePath(imagesRoot, MediaAssetVariantType.Sample)));
        Assert.True(File.Exists(asset.GetFilePath(imagesRoot, MediaAssetVariantType.Thumbnail)));
    }

    [Fact]
    public void ProcessFile_UnknownExtension_Throws()
    {
        using var tempDir = new TemporaryDirectory();
        var fixturePath = getFixturePath("image.jpg");
        var imagesRoot = tempDir.GetPath("images");

        var filePath = tempDir.GetPath("image.asd");
        File.Copy(fixturePath, filePath);
        using var stream = File.OpenRead(filePath);
        Assert.Throws<InvalidOperationException>(() => MediaFileProcessor.ProcessFile(stream, imagesRoot));
    }

    [Fact]
    public void ProcessFile_CorruptedFile_Throws()
    {
        using var tempDir = new TemporaryDirectory();
        var fixturePath = getFixturePath("image_corrupted.jpg");
        var imagesRoot = tempDir.GetPath("images");

        using var stream = File.OpenRead(fixturePath);
        Assert.Throws<VipsException>(() => MediaFileProcessor.ProcessFile(stream, imagesRoot));
    }

    [Fact]
    public void ProcessFile_SamePixelHash_PixelHashesEqual()
    {
        using var tempDir = new TemporaryDirectory();
        var imagesRoot = tempDir.GetPath("images");

        var imagePath1 = getFixturePath("image.jpg");
        var imagePath2 = getFixturePath("image_metadata.jpg");

        using var stream1 = File.OpenRead(imagePath1);
        var asset1 = MediaFileProcessor.ProcessFile(stream1, imagesRoot);

        using var stream2 = File.OpenRead(imagePath2);
        var asset2 = MediaFileProcessor.ProcessFile(stream2, imagesRoot);

        Assert.NotEqual(asset1.Md5, asset2.Md5);
        Assert.Equal(asset1.PixelHash, asset2.PixelHash);
    }

    [Fact]
    public void ProcessFile_DifferentPixelHash_PixelHashesDiffer()
    {
        using var tempDir = new TemporaryDirectory();
        var imagesRoot = tempDir.GetPath("images");

        var imagePath1 = getFixturePath("image.jpg");
        var imagePath2 = getFixturePath("image2.jpg");

        using var stream1 = File.OpenRead(imagePath1);
        var asset1 = MediaFileProcessor.ProcessFile(stream1, imagesRoot);

        using var stream2 = File.OpenRead(imagePath2);
        var asset2 = MediaFileProcessor.ProcessFile(stream2, imagesRoot);

        Assert.NotEqual(asset1.PixelHash, asset2.PixelHash);
    }
}
