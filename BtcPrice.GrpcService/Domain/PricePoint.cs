namespace BtcPrice.GrpcService.Domain;

/// <summary>
/// Represents a Bitcoin price point normalized to hourly UTC precision.
/// </summary>
public sealed record PricePoint(DateTime TimestampUtc, decimal Price)
{
    /// <summary>
    /// Creates a new price point with normalized timestamp.
    /// </summary>
    public static PricePoint Create(DateTime timestamp, decimal price) =>
        new(NormalizeToHourUtc(timestamp), price);

    /// <summary>
    /// Normalizes a timestamp to the start of its hour in UTC.
    /// </summary>
    public static DateTime NormalizeToHourUtc(DateTime timestamp)
    {
        var utc = timestamp.Kind switch
        {
            DateTimeKind.Utc => timestamp,
            DateTimeKind.Local => timestamp.ToUniversalTime(),
            _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)
        };

        return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, DateTimeKind.Utc);
    }
}

