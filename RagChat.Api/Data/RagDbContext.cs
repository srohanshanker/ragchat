using Microsoft.EntityFrameworkCore;
using RagChat.Api.Models;

namespace RagChat.Api.Data;

public class RagDbContext : DbContext
{
    public RagDbContext(DbContextOptions<RagDbContext> options) : base(options)
    {
    }

    public DbSet<DocumentEntity> Documents => Set<DocumentEntity>();
    public DbSet<DocumentChunkEntity> DocumentChunks => Set<DocumentChunkEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentEntity>()
            .HasMany(d => d.Chunks)
            .WithOne(c => c.Document)
            .HasForeignKey(c => c.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
