using Microsoft.EntityFrameworkCore;
using Torri.Data.Entities;

namespace Torri.Data;

public class TorriContext(DbContextOptions<TorriContext> options) : DbContext(options)
{
    public DbSet<TorrentEntity> Torrents { get; set; }
    public DbSet<VideoFileEntity> VideoFiles { get; set; }
    public DbSet<SerieMaskEntity> SerieMasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder
            .Entity<TorrentEntity>()
            .Property(e => e.TorrentType)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder
            .Entity<TorrentEntity>()
            .HasIndex(e => e.Hash)
            .IsUnique();
    }
    
    public IUpdateHelper<TEntity> BeginUpdate<TEntity>(TEntity entity)
        where TEntity : class
    {
        var entry = Attach(entity);
        return new UpdateHelper<TEntity>(entry, this);
    }
}