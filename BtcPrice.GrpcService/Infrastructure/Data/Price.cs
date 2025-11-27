namespace BtcPrice.GrpcService.Infrastructure.Data;

/// <summary>
/// Represents a persisted Bitcoin price record.
/// </summary>
public sealed class Price
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the timestamp in UTC (normalized to hour).
    /// </summary>
    public required DateTime TimestampUtc { get; set; }

    /// <summary>
    /// Gets or sets the price value.
    /// </summary>
    public required decimal PriceValue { get; set; }
}

