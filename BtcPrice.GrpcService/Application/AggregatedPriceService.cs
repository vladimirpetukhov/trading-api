using BtcPrice.GrpcService.Domain;

namespace BtcPrice.GrpcService.Application;

/// <summary>
/// Aggregates Bitcoin prices from multiple providers and manages caching.
/// </summary>
public sealed class AggregatedPriceService(
    IEnumerable<IPriceProvider> providers,
    IPriceRepository repository) : IAggregatedPriceService
{
    private readonly IReadOnlyList<IPriceProvider> _providers = providers.ToArray();

    public async Task<PricePoint> GetAggregatedPriceAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        var hour = PricePoint.NormalizeToHourUtc(timestamp);

        var existing = await repository.GetAsync(hour, cancellationToken);
        if (existing is not null)
            return existing;

        var prices = await FetchPricesFromAllProvidersAsync(hour, cancellationToken);

        if (prices.Count == 0)
            throw new PriceNotFoundException(hour);

        var average = CalculateAverage(prices);
        var pricePoint = PricePoint.Create(hour, average);

        await repository.SaveAsync(pricePoint, cancellationToken);
        return pricePoint;
    }

    public Task<IReadOnlyList<PricePoint>> GetPriceHistoryAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        var fromHour = PricePoint.NormalizeToHourUtc(from);
        var toHour = PricePoint.NormalizeToHourUtc(to);

        if (toHour < fromHour)
            throw new ArgumentException("To must be greater than or equal to From.", nameof(to));

        return repository.GetRangeAsync(fromHour, toHour, cancellationToken);
    }

    private async Task<List<decimal>> FetchPricesFromAllProvidersAsync(
        DateTime hour,
        CancellationToken cancellationToken)
    {
        var tasks = _providers
            .Select(provider => provider.GetPriceAsync(hour, cancellationToken))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        return results
            .Where(price => price.HasValue)
            .Select(price => price!.Value)
            .ToList();
    }

    private static decimal CalculateAverage(IReadOnlyList<decimal> prices)
    {
        var sum = prices.Aggregate(decimal.Zero, (current, value) => current + value);
        return sum / prices.Count;
    }
}

