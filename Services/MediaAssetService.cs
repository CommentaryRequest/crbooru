using System.IO;
using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;
using CRbooru.Logical;

namespace CRbooru.Services;

public class MediaAssetService
{
    private readonly CRbooruContext _context;
    private readonly IWebHostEnvironment _webHostEnv;

    public MediaAssetService(CRbooruContext context, IWebHostEnvironment webHostEnv)
    {
        _context = context;
        _webHostEnv = webHostEnv;
    }

    public ValueTask<MediaAsset> Get(int id)
    {
        return _context.MediaAssets.FindAsync(id);
    }

    public Task<MediaAsset> ByMd5(string md5)
    {
        return _context.MediaAssets.FirstOrDefaultAsync(m => md5 == m.Md5);
    }

    public async Task<MediaAsset> CreateFromFile(IFormFile file)
    {
        string tempPath = Path.GetTempFileName() + Path.GetExtension(file.FileName);
        using (var stream = new FileStream(tempPath, FileMode.Create)) {
            await file.CopyToAsync(stream);
        }

        using (var stream = File.OpenRead(tempPath)) {
            var md5 = MediaFileProcessor.ComputeFileMd5(stream);
            var assetByMd5 = await ByMd5(md5);
            if (assetByMd5 != null) {
                return assetByMd5;
            }
            var mediaAsset = MediaFileProcessor.ProcessFile(stream, Path.Combine(_webHostEnv.WebRootPath, "images"));
            _context.MediaAssets.Add(mediaAsset);
            await _context.SaveChangesAsync();
            return mediaAsset;
        }
    }
}
