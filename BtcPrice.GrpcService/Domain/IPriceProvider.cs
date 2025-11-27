namespace BtcPrice.GrpcService.Domain;

/// <summary>
/// Provides Bitcoin price data from an external source.
/// </summary>
public interface IPriceProvider
{
    /// <summary>
    /// Gets the Bitcoin price for a specific hour.
    /// </summary>
    /// <param name="hourUtc">The hour in UTC for which to retrieve the price.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The price if available; otherwise null.</returns>
    Task<decimal?> GetPriceAsync(DateTime hourUtc, CancellationToken cancellationToken);
}

