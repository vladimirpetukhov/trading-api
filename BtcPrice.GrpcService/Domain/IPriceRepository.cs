namespace BtcPrice.GrpcService.Domain;

/// <summary>
/// Provides persistence operations for price data.
/// </summary>
public interface IPriceRepository
{
    /// <summary>
    /// Retrieves a price point for a specific timestamp.
    /// </summary>
    /// <param name="timestampUtc">The timestamp in UTC.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The price point if found; otherwise null.</returns>
    Task<PricePoint?> GetAsync(DateTime timestampUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all price points within a time range.
    /// </summary>
    /// <param name="fromUtc">The start of the range in UTC.</param>
    /// <param name="toUtc">The end of the range in UTC.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only list of price points.</returns>
    Task<IReadOnlyList<PricePoint>> GetRangeAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Saves or updates a price point.
    /// </summary>
    /// <param name="pricePoint">The price point to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SaveAsync(PricePoint pricePoint, CancellationToken cancellationToken);
}

