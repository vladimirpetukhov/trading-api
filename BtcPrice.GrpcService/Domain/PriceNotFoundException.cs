namespace BtcPrice.GrpcService.Domain;

/// <summary>
/// Thrown when a price cannot be found for a given timestamp.
/// </summary>
public sealed class PriceNotFoundException(DateTime timestampUtc)
    : Exception("Price not found for timestamp")
{
    /// <summary>
    /// Gets the normalized timestamp for which the price was not found.
    /// </summary>
    public DateTime TimestampUtc { get; } = PricePoint.NormalizeToHourUtc(timestampUtc);
}

