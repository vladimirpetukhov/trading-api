using Microsoft.EntityFrameworkCore;

namespace BtcPrice.GrpcService.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for price data.
/// </summary>
public sealed class PriceDbContext(DbContextOptions<PriceDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the prices table.
    /// </summary>
    public DbSet<Price> Prices => Set<Price>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigurePriceEntity(modelBuilder);
    }

    private static void ConfigurePriceEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Price>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TimestampUtc)
                .IsRequired()
                .HasColumnType("TEXT");

            entity.Property(e => e.PriceValue)
                .IsRequired()
                .HasColumnType("REAL");

            entity.HasIndex(e => e.TimestampUtc)
                .IsUnique();
        });
    }
}

