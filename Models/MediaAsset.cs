using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace CRbooru.Models;

public enum MediaAssetVariantType
{
    Original,
    Sample,
    Thumbnail
}

[Index(nameof(Md5), IsUnique = true)]
public class MediaAsset
{
    public int Id { get; private set; }
    [MaxLength(32)]
    public string Md5 { get; private set; }
    public string PixelHash { get; private set; }
    public string FileType { get; private set; }
    public long FileSize { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public MediaAsset(string md5, string pixelHash, string fileType, long fileSize, int width, int height)
    {
        Md5 = md5;
        PixelHash = pixelHash;
        FileType = fileType;
        FileSize = fileSize;
        Width = width;
        Height = height;
    }

    public string GetFilePath(string basePath, MediaAssetVariantType variant)
    {
        string variantDir = variant switch
        {
            MediaAssetVariantType.Original => "original",
            MediaAssetVariantType.Sample => "sample",
            MediaAssetVariantType.Thumbnail => "thumb"
        };
        string fileType = variant == MediaAssetVariantType.Original ? FileType : "jpg";
        return Path.Combine(basePath, variantDir, Md5[0..2], Md5[2..4], $"{Md5}.{fileType}");
    }
}
