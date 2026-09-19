using Microsoft.EntityFrameworkCore;
using CRbooru.Models;

namespace CRbooru.Data;

public class CRbooruContext : DbContext
{
    public CRbooruContext(DbContextOptions<CRbooruContext> options)
        : base(options)
    {
    }

    public DbSet<MediaAsset> MediaAssets { get; set; }
}
