using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;
using CRbooru.Logical;

namespace CRbooru.Services;

public class MediaAssetService
{
    private readonly CRbooruContext _context;
    private readonly StorageOptions _storageOptions;

    public MediaAssetService(CRbooruContext context, StorageOptions storageOptions)
    {
        _context = context;
        _storageOptions = storageOptions;
    }

    public ValueTask<MediaAsset?> Get(int id)
    {
        return _context.MediaAssets.FindAsync(id);
    }

    public Task<MediaAsset?> ByMd5(string md5)
    {
        return _context.MediaAssets.FirstOrDefaultAsync(m => md5 == m.Md5);
    }

    public async Task<List<MediaAsset>> ListAsync(int limit)
    {
        return await _context.MediaAssets
            .OrderByDescending(p => p.Id)
            .Take(limit)
            .ToListAsync();
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
            var mediaAsset = MediaFileProcessor.ProcessFile(stream, _storageOptions.ImagesPath);
            _context.MediaAssets.Add(mediaAsset);
            await _context.SaveChangesAsync();
            return mediaAsset;
        }
    }
}
