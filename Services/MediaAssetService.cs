using CRbooru.Data;
using CRbooru.Models;

namespace CRbooru.Services;

public class MediaAssetService
{
    private readonly CRbooruContext _context;

    public MediaAssetService(CRbooruContext context)
    {
        _context = context;
    }

    public ValueTask<MediaAsset> Get(int id)
    {
        return _context.MediaAssets.FindAsync(id);
    }
}
