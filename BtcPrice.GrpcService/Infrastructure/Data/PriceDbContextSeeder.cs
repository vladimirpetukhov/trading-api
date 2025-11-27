using Microsoft.EntityFrameworkCore;

namespace BtcPrice.GrpcService.Infrastructure.Data;

/// <summary>
/// Seeds initial price data into the database.
/// </summary>
public static class PriceDbContextSeeder
{
    /// <summary>
    /// Seeds the database with initial price data if it's empty.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public static async Task SeedAsync(PriceDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Prices.AnyAsync(cancellationToken))
            return;

        var seedData = GetSeedData();
        await context.Prices.AddRangeAsync(seedData, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static List<Price> GetSeedData() =>
    [
        new() { TimestampUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), PriceValue = 42000m },
        new() { TimestampUtc = new DateTime(2024, 1, 1, 1, 0, 0, DateTimeKind.Utc), PriceValue = 42100m },
        new() { TimestampUtc = new DateTime(2024, 1, 1, 2, 0, 0, DateTimeKind.Utc), PriceValue = 42200m },
        new() { TimestampUtc = new DateTime(2024, 1, 1, 3, 0, 0, DateTimeKind.Utc), PriceValue = 42150m },
        new() { TimestampUtc = new DateTime(2024, 1, 1, 4, 0, 0, DateTimeKind.Utc), PriceValue = 42300m },
        new() { TimestampUtc = new DateTime(2024, 1, 1, 5, 0, 0, DateTimeKind.Utc), PriceValue = 42250m },
    ];
}

