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
    public string FileType { get; private set; }

    public MediaAsset(string md5, string fileType)
    {
        Md5 = md5;
        FileType = fileType;
    }

    public string GetFilePath(string basePath, MediaAssetVariantType variant)
    {
        string variantDir = variant switch
        {
            MediaAssetVariantType.Original => "original",
            MediaAssetVariantType.Sample => "sample",
            MediaAssetVariantType.Thumbnail => "thumb"
        };
        return Path.Combine(basePath, variantDir, Md5[0..2], Md5[2..4], $"{Md5}.{FileType}");
    }
}
