using System.Linq;
using System.Security.Cryptography;
using CRbooru.Models;
using NetVips;

namespace CRbooru.Logical;

public static class MediaFileProcessor
{
    public static readonly string[] SUPPORTED_FILE_TYPES = [
        "png", "jpg", "webp", "avif", "gif"
    ];

    public static string ComputeFileMd5(FileStream fileStream)
    {
        using var md5 = MD5.Create();
        fileStream.Position = 0;
        byte[] fileHashBytes = md5.ComputeHash(fileStream);
        return Convert.ToHexString(fileHashBytes).ToLowerInvariant();
    }

    public static MediaAsset ProcessFile(FileStream fileStream, string imagesRoot)
    {
        // Check if it's a supported file type
        string extension = Path.GetExtension(fileStream.Name).ToLower()[1..];
        if (!SUPPORTED_FILE_TYPES.Contains(extension)) {
            throw new InvalidOperationException("Unsupported file type");
        }

        fileStream.Position = 0;
        using var image = Image.NewFromStream(fileStream);
        var width = image.Width;
        var height = image.Height;

        // Get the file's md5 hash
        string md5Hash = ComputeFileMd5(fileStream);

        // Get the image's pixel hash
        using var md5 = MD5.Create();
        byte[] pixelData = image.WriteToMemory();
        byte[] pixelHashBytes = md5.ComputeHash(pixelData);
        string pixelHash = Convert.ToHexString(pixelHashBytes).ToLowerInvariant();

        MediaAsset mediaAsset = new(md5Hash, pixelHash, extension, fileStream.Length, width, height);

        // Generate thumbnail and sample image
        using var thumbnail = image.ThumbnailImage(360, 360);
        using var sample = image.ThumbnailImage(850);

        // Compute paths and create required dirs
        string originalPath = mediaAsset.GetFilePath(imagesRoot, MediaAssetVariantType.Original);
        string samplePath = mediaAsset.GetFilePath(imagesRoot, MediaAssetVariantType.Sample);
        string thumbnailPath = mediaAsset.GetFilePath(imagesRoot, MediaAssetVariantType.Thumbnail);
        string originalDir = Path.GetDirectoryName(originalPath)!;
        string sampleDir = Path.GetDirectoryName(samplePath)!;
        string thumbnailDir = Path.GetDirectoryName(thumbnailPath)!;
        Directory.CreateDirectory(originalDir);
        Directory.CreateDirectory(sampleDir);
        Directory.CreateDirectory(thumbnailDir);

        // Save the images
        image.WriteToFile(originalPath);
        sample.WriteToFile(samplePath);
        thumbnail.WriteToFile(thumbnailPath);

        return mediaAsset;
    }
}
