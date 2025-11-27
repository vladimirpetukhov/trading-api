using BtcPrice.GrpcService.Application;
using BtcPrice.GrpcService.Domain;
using DomainPricePoint = BtcPrice.GrpcService.Domain.PricePoint;

namespace BtcPrice.GrpcService.Tests;

public sealed class AggregatedPriceServiceTests
{
    [Fact]
    public async Task ReturnsCachedPriceWhenExists()
    {
        var timestamp = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var cached = new DomainPricePoint(timestamp, 1000m);

        var repository = new InMemoryRepository(cached);
        var providers = Array.Empty<IPriceProvider>();
        var service = new AggregatedPriceService(providers, repository);

        var result = await service.GetAggregatedPriceAsync(timestamp, CancellationToken.None);

        Assert.Equal(cached, result);
    }

    [Fact]
    public async Task AggregatesWhenNotCached()
    {
        var timestamp = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        var repository = new InMemoryRepository(null);
        var provider1 = new StubProvider(1000m);
        var provider2 = new StubProvider(1100m);
        var service = new AggregatedPriceService(new IPriceProvider[] { provider1, provider2 }, repository);

        var result = await service.GetAggregatedPriceAsync(timestamp, CancellationToken.None);

        Assert.Equal(1050m, result.Price);
    }

    [Fact]
    public async Task ThrowsWhenNoProviderHasPrice()
    {
        var timestamp = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        var repository = new InMemoryRepository(null);
        var provider = new StubProvider(null);
        var service = new AggregatedPriceService(new IPriceProvider[] { provider }, repository);

        await Assert.ThrowsAsync<PriceNotFoundException>(() => service.GetAggregatedPriceAsync(timestamp, CancellationToken.None));
    }

    sealed class InMemoryRepository : IPriceRepository
    {
        readonly DomainPricePoint? value;

        public InMemoryRepository(DomainPricePoint? value)
        {
            this.value = value;
        }

        public Task<DomainPricePoint?> GetAsync(DateTime timestampUtc, CancellationToken cancellationToken)
        {
            return Task.FromResult(value);
        }

        public Task<IReadOnlyList<DomainPricePoint>> GetRangeAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken)
        {
            var result = value == null ? Array.Empty<DomainPricePoint>() : new[] { value };
            return Task.FromResult<IReadOnlyList<DomainPricePoint>>(result);
        }

        public Task SaveAsync(DomainPricePoint pricePoint, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    sealed class StubProvider : IPriceProvider
    {
        readonly decimal? price;

        public StubProvider(decimal? price)
        {
            this.price = price;
        }

        public Task<decimal?> GetPriceAsync(DateTime hourUtc, CancellationToken cancellationToken)
        {
            return Task.FromResult(price);
        }
    }
}

