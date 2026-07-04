using connector_daemon.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace connector_daemon.Persistence;

internal sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<JobEventEntity> JobEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobEventEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Build)
                .IsRequired();

            entity.Property(e => e.Commit)
                .IsRequired()
                .HasMaxLength(40);

            entity.Property(e => e.JobEvent)
                .IsRequired()
                .HasMaxLength(40);

            entity.HasIndex(e => new
                {
                    e.Name,
                    e.Build,
                    e.Commit,
                    e.JobEvent,
                })
                .IsUnique();

            entity.Property(e => e.GitUrl)
                .HasMaxLength(500);

            entity.Property(e => e.Url)
                .HasMaxLength(500);

            entity.Property(e => e.RegisteredAt)
                .IsRequired()
                .HasMaxLength(40);

            entity.Property(e => e.SyncStatus)
                .IsRequired()
                .HasMaxLength(40);

            entity.Property(e => e.EnqueuedAt)
                .HasMaxLength(40);

            entity.Property(e => e.FinishedAt)
                .HasMaxLength(40);
        });

        base.OnModelCreating(modelBuilder);
    }
}