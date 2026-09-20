using Microsoft.EntityFrameworkCore;
using CRbooru.Data;
using CRbooru.Models;

namespace CRbooru.Services;

public class UploadService
{
    private readonly CRbooruContext _context;
    private readonly MediaAssetService _assets;

    public UploadService(CRbooruContext context, MediaAssetService assets)
    {
        _context = context;
        _assets = assets;
    }

    public Task<Upload> Get(int id)
    {
        return _context.Uploads
            .Include(u => u.MediaAssets)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Upload> CreateAsync(User uploader, IEnumerable<IFormFile> files)
    {
        var upload = new Upload {
            Uploader = uploader,
            Status = UploadStatus.Pending,
            MediaAssets = new()
        };

        _context.Uploads.Add(upload);
        await _context.SaveChangesAsync();

        try {
            foreach (var file in files) {
                var mediaAsset = await _assets.CreateFromFile(file);
                upload.MediaAssets.Add(mediaAsset);
            }
            upload.Status = UploadStatus.Success;
        } catch (Exception exc) {
            upload.Status = UploadStatus.Error;
            upload.StatusMessage = exc.Message;
            Console.WriteLine($"failed upload: {exc.ToString()}");
        }

        await _context.SaveChangesAsync();
        return upload;
    }
}
