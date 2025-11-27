using System.Globalization;
using System.Text.Json;
using BtcPrice.GrpcService.Domain;

namespace BtcPrice.GrpcService.Infrastructure;

/// <summary>
/// Fetches Bitcoin prices from the Bitstamp API.
/// </summary>
public sealed class BitstampPriceProvider(HttpClient client) : IPriceProvider
{
    private const string OhlcEndpoint = "api/v2/ohlc/btcusd/?step=3600&limit=1&start={0}";

    public async Task<decimal?> GetPriceAsync(DateTime hourUtc, CancellationToken cancellationToken)
    {
        var normalized = PricePoint.NormalizeToHourUtc(hourUtc);
        var epochSeconds = new DateTimeOffset(normalized).ToUnixTimeSeconds();
        var uri = string.Format(OhlcEndpoint, epochSeconds);

        using var response = await client.GetAsync(uri, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        return await ExtractPriceFromResponseAsync(response, cancellationToken);
    }

    private static async Task<decimal?> ExtractPriceFromResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var data = await JsonSerializer.DeserializeAsync<BitstampOhlcResponse>(
            stream,
            cancellationToken: cancellationToken);

        var closeString = data?.Data?.Ohlc?.FirstOrDefault()?.Close;
        if (string.IsNullOrWhiteSpace(closeString))
            return null;

        return decimal.TryParse(
            closeString,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var close)
            ? close
            : null;
    }

    private sealed record BitstampOhlcResponse(BitstampOhlcData? Data);
    private sealed record BitstampOhlcData(IReadOnlyList<BitstampOhlcItem>? Ohlc);
    private sealed record BitstampOhlcItem(string? Close);
}

