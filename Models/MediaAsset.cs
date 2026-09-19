using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace CRbooru.Models;

[Index(nameof(Md5), IsUnique = true)]
public class MediaAsset
{
    public int Id { get; private set; }
    [MaxLength(32)]
    public string Md5 { get; set; }
    public string FileType { get; set; }

    public string GetFilePath(string basePath)
    {
        return Path.Combine(basePath, Md5[0..2], Md5[2..4], $"{Md5}.{FileType}");
    }
}
