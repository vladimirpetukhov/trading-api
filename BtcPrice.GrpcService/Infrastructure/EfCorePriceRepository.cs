using BtcPrice.GrpcService.Domain;
using BtcPrice.GrpcService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BtcPrice.GrpcService.Infrastructure;

/// <summary>
/// EF Core implementation of the price repository using SQLite.
/// </summary>
public sealed class EfCorePriceRepository(PriceDbContext context) : IPriceRepository
{
    public async Task<PricePoint?> GetAsync(DateTime timestampUtc, CancellationToken cancellationToken)
    {
        var normalized = PricePoint.NormalizeToHourUtc(timestampUtc);

        var price = await context.Prices
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.TimestampUtc == normalized, cancellationToken);

        return price is null
            ? null
            : new PricePoint(price.TimestampUtc, price.PriceValue);
    }

    public async Task<IReadOnlyList<PricePoint>> GetRangeAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken)
    {
        var normalizedFrom = PricePoint.NormalizeToHourUtc(fromUtc);
        var normalizedTo = PricePoint.NormalizeToHourUtc(toUtc);

        var prices = await context.Prices
            .AsNoTracking()
            .Where(p => p.TimestampUtc >= normalizedFrom && p.TimestampUtc <= normalizedTo)
            .OrderBy(p => p.TimestampUtc)
            .ToListAsync(cancellationToken);

        return prices
            .Select(p => new PricePoint(p.TimestampUtc, p.PriceValue))
            .ToList();
    }

    public async Task SaveAsync(PricePoint pricePoint, CancellationToken cancellationToken)
    {
        var normalized = PricePoint.NormalizeToHourUtc(pricePoint.TimestampUtc);

        var existing = await context.Prices
            .FirstOrDefaultAsync(p => p.TimestampUtc == normalized, cancellationToken);

        if (existing is not null)
        {
            existing.PriceValue = pricePoint.Price;
        }
        else
        {
            context.Prices.Add(new Price
            {
                TimestampUtc = normalized,
                PriceValue = pricePoint.Price
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}

