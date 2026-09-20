using Microsoft.EntityFrameworkCore;
using CRbooru.Models;

namespace CRbooru.Data;

public class CRbooruContext : DbContext
{
    public CRbooruContext(DbContextOptions<CRbooruContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(u => u.Name)
            .HasColumnType("TEXT COLLATE NOCASE"); // TODO this is sqlite only

        modelBuilder.Entity<Post>()
            .HasMany(p => p.Tags)
            .WithMany()
            .UsingEntity(j => j.ToTable("PostTags"));
    }

    public DbSet<MediaAsset> MediaAssets { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Upload> Uploads { get; set; }
}
