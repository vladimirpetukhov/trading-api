using FluentValidation;
using JetBrains.Annotations;

namespace BtcPrice.GrpcService.Endpoints.Validators;

public sealed class GetAggregatedPriceQueryValidator : AbstractValidator<GetAggregatedPriceQuery>
{
    public GetAggregatedPriceQueryValidator()
    {
        RuleFor(x => x.Timestamp)
            .NotEmpty()
            .WithMessage("Timestamp is required");
    }
}


/// <summary>
/// Validates the price history query.
/// </summary>
[UsedImplicitly]
public sealed class GetPriceHistoryQueryValidator : AbstractValidator<GetPriceHistoryQuery>
{
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>
    public GetPriceHistoryQueryValidator()
    {
        RuleFor(x => x.From)
            .NotEmpty()
            .WithMessage("From date is required");

        RuleFor(x => x.To)
            .NotEmpty()
            .WithMessage("To date is required");

        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From)
            .WithMessage("To date must be greater than or equal to From date");
    }
}

public sealed record GetAggregatedPriceQuery(DateTime Timestamp);

public sealed record GetPriceHistoryQuery(DateTime From, DateTime To);

