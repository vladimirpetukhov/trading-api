using BtcPrice.GrpcService.Domain;

namespace BtcPrice.GrpcService.Application;

/// <summary>
/// Provides aggregated Bitcoin price data from multiple sources.
/// </summary>
public interface IAggregatedPriceService
{
    /// <summary>
    /// Gets the aggregated price for a specific timestamp from all providers.
    /// </summary>
    /// <param name="timestamp">The timestamp for which to retrieve the price.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The aggregated price point.</returns>
    /// <exception cref="PriceNotFoundException">Thrown when no price data is available.</exception>
    Task<PricePoint> GetAggregatedPriceAsync(DateTime timestamp, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the price history for a time range.
    /// </summary>
    /// <param name="from">The start of the range.</param>
    /// <param name="to">The end of the range.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only list of price points.</returns>
    Task<IReadOnlyList<PricePoint>> GetPriceHistoryAsync(DateTime from, DateTime to, CancellationToken cancellationToken);
}

