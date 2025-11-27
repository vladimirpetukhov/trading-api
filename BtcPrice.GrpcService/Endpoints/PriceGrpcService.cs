using BtcPrice.GrpcService.Application;
using BtcPrice.GrpcService.Domain;
using BtcPrice.GrpcService.Services;
using Grpc.Core;

namespace BtcPrice.GrpcService.Endpoints;

/// <summary>
/// gRPC service for price operations.
/// </summary>
public sealed class PriceGrpcService(IAggregatedPriceService service) : PriceService.PriceServiceBase
{
    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public override Task<Services.PriceResponse> GetAggregatedPrice(
        Services.GetAggregatedPriceRequest request,
        ServerCallContext context) =>
        GetAggregatedPriceInternal(request, context);

    private async Task<Services.PriceResponse> GetAggregatedPriceInternal(
        Services.GetAggregatedPriceRequest request,
        ServerCallContext context)
    {
        try
        {
            var timestamp = UnixTimeStampToDateTime(request.TimestampUnix);
            var pricePoint = await service.GetAggregatedPriceAsync(timestamp, context.CancellationToken);

            return new Services.PriceResponse
            {
                TimestampUtc = DateTimeToUnixTimeStamp(pricePoint.TimestampUtc),
                Price = (double)pricePoint.Price
            };
        }
        catch (PriceNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Price not found"));
        }
    }

    public override Task<Services.PriceHistoryResponse> GetPriceHistory(
        Services.GetPriceHistoryRequest request,
        ServerCallContext context) =>
        GetPriceHistoryInternal(request, context);

    private async Task<Services.PriceHistoryResponse> GetPriceHistoryInternal(
        Services.GetPriceHistoryRequest request,
        ServerCallContext context)
    {
        var from = UnixTimeStampToDateTime(request.FromUnix);
        var to = UnixTimeStampToDateTime(request.ToUnix);

        var prices = await service.GetPriceHistoryAsync(from, to, context.CancellationToken);

        var response = new Services.PriceHistoryResponse();
        foreach (var price in prices)
        {
            response.Prices.Add(new Services.PriceResponse
            {
                TimestampUtc = DateTimeToUnixTimeStamp(price.TimestampUtc),
                Price = (double)price.Price
            });
        }

        return response;
    }

    private static DateTime UnixTimeStampToDateTime(long unixTimeStamp) =>
        UnixEpoch.AddSeconds(unixTimeStamp).ToUniversalTime();

    private static long DateTimeToUnixTimeStamp(DateTime dateTime) =>
        (long)(dateTime - UnixEpoch).TotalSeconds;
}

