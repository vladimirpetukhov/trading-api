using System.Text.Json;
using BtcPrice.GrpcService.Domain;

namespace BtcPrice.GrpcService.Infrastructure;

/// <summary>
/// Fetches Bitcoin prices from the Bitfinex API.
/// </summary>
public sealed class BitfinexPriceProvider(HttpClient client) : IPriceProvider
{
    private const string CandlesEndpoint = "v2/candles/trade:1h:tBTCUSD/hist?start={0}&end={0}&limit=1";
    private const int CloseElementIndex = 2;
    private const int MinArrayLength = 3;

    public async Task<decimal?> GetPriceAsync(DateTime hourUtc, CancellationToken cancellationToken)
    {
        var normalized = PricePoint.NormalizeToHourUtc(hourUtc);
        var startMs = new DateTimeOffset(normalized).ToUnixTimeMilliseconds();
        var uri = string.Format(CandlesEndpoint, startMs);

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
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (document.RootElement.ValueKind is not JsonValueKind.Array)
            return null;

        var first = document.RootElement.EnumerateArray().FirstOrDefault();
        if (first.ValueKind is not JsonValueKind.Array || first.GetArrayLength() < MinArrayLength)
            return null;

        var closeElement = first[CloseElementIndex];
        if (closeElement.ValueKind is not JsonValueKind.Number)
            return null;

        return (decimal)closeElement.GetDouble();
    }
}

