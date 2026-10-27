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
            .HasColumnType("citext");

        modelBuilder.Entity<Post>()
            .HasMany(p => p.Tags)
            .WithMany()
            .UsingEntity(j => j.ToTable("PostTags"));

        modelBuilder.Entity<Post>()
            .HasOne(p => p.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(p => p.ParentId);

        modelBuilder.Entity<Upload>()
            .HasMany(u => u.MediaAssets)
            .WithMany()
            .UsingEntity(j => j.ToTable("UploadMediaAssets"));

        modelBuilder.Entity<PostVersion>()
            .HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId);

        modelBuilder.Entity<PostVersion>()
            .HasOne(v => v.Post)
            .WithMany()
            .HasForeignKey(v => v.PostId);

        modelBuilder.Entity<PoolPost>()
            .HasKey(p => new { p.PoolId, p.PostId });

        modelBuilder.Entity<PoolPost>()
            .HasOne<Pool>()
            .WithMany()
            .HasForeignKey(p => p.PoolId);

        modelBuilder.Entity<PoolPost>()
            .HasOne<Post>()
            .WithMany()
            .HasForeignKey(p => p.PostId);

        modelBuilder.Entity<PoolPost>()
            .HasIndex(p => p.PostId);

        modelBuilder.Entity<PoolVersion>()
            .HasOne(v => v.Pool)
            .WithMany()
            .HasForeignKey(v => v.PoolId);

        modelBuilder.Entity<PoolVersion>()
            .HasOne(v => v.Updater)
            .WithMany()
            .HasForeignKey(v => v.UpdaterId);
    }

    public DbSet<MediaAsset> MediaAssets { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Upload> Uploads { get; set; }
    public DbSet<PostVersion> PostVersions { get; set; }
    public DbSet<Pool> Pools { get; set; }
    public DbSet<PoolPost> PoolPosts { get; set; }
    public DbSet<PoolVersion> PoolVersions { get; set; }
}
