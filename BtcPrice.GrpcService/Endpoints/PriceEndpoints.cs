using BtcPrice.GrpcService.Application;
using BtcPrice.GrpcService.Domain;
using BtcPrice.GrpcService.Endpoints.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BtcPrice.GrpcService.Endpoints;

/// <summary>
/// Defines REST API endpoints for price operations.
/// </summary>
public static class PriceEndpoints
{
    private const string PricesGroup = "/api/prices";
    private const string PricesGroupName = "Prices";

    /// <summary>
    /// Maps price endpoints to the application.
    /// </summary>
    public static void MapPriceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(PricesGroup)
            .WithName(PricesGroupName);

        group.MapGet("/aggregated", GetAggregatedPrice)
            .WithName("GetAggregatedPrice")
            .WithDescription("Gets the aggregated BTC/USD price for a specific hour")
            .Produces<Result<PriceResponse>>(StatusCodes.Status200OK)
            .Produces<Result<PriceResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/history", GetPriceHistory)
            .WithName("GetPriceHistory")
            .WithDescription("Gets all persisted hourly prices in a time range")
            .Produces<Result<List<PriceResponse>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<Result<PriceResponse>> GetAggregatedPrice(
        [FromQuery] DateTime timestamp,
        IAggregatedPriceService service,
        IValidator<GetAggregatedPriceQuery> validator,
        CancellationToken cancellationToken)
    {
        var query = new GetAggregatedPriceQuery(timestamp);
        await validator.ValidateAndThrowAsync(query, cancellationToken);

        try
        {
            var pricePoint = await service.GetAggregatedPriceAsync(timestamp, cancellationToken);
            return new Result<PriceResponse>.Success(
                new PriceResponse(pricePoint.TimestampUtc, pricePoint.Price));
        }
        catch (PriceNotFoundException)
        {
            return new Result<PriceResponse>.NotFound();
        }
    }

    private static async Task<Result<List<PriceResponse>>> GetPriceHistory(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        IAggregatedPriceService service,
        IValidator<GetPriceHistoryQuery> validator,
        CancellationToken cancellationToken)
    {
        var query = new GetPriceHistoryQuery(from, to);
        await validator.ValidateAndThrowAsync(query, cancellationToken);

        var prices = await service.GetPriceHistoryAsync(from, to, cancellationToken);
        var result = prices
            .Select(p => new PriceResponse(p.TimestampUtc, p.Price))
            .ToList();

        return new Result<List<PriceResponse>>.Success(result);
    }
}

/// <summary>
/// Represents a price response in the API.
/// </summary>
public sealed record PriceResponse(DateTime Timestamp, decimal Price);

