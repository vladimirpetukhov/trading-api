using BtcPrice.GrpcService.Application;
using BtcPrice.GrpcService.Domain;
using BtcPrice.GrpcService.Endpoints.Validators;
using BtcPrice.GrpcService.Infrastructure;
using BtcPrice.GrpcService.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;

namespace BtcPrice.GrpcService;

/// <summary>
/// Extension methods for service collection configuration.
/// </summary>
internal static class ServiceCollectionExtensions
{
    private const string BitstampBaseAddress = "https://www.bitstamp.net/";
    private const string BitfinexBaseAddress = "https://api-pub.bitfinex.com/";
    private const string DefaultConnectionString = "Data Source=prices.db";

    /// <summary>
    /// Adds presentation layer services (gRPC, Swagger, etc.).
    /// </summary>
    internal static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddGrpc();
        services.AddGrpcReflection();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "BTC Price Service", Version = "v1" });
            c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
        });

        return services;
    }

    /// <summary>
    /// Adds application layer services.
    /// </summary>
    internal static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<GetAggregatedPriceQueryValidator>();
        services.AddScoped<IAggregatedPriceService, AggregatedPriceService>();

        return services;
    }

    /// <summary>
    /// Adds infrastructure layer services.
    /// </summary>
    internal static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Sqlite") ?? DefaultConnectionString;
        services.AddDbContext<PriceDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddHttpClient<BitstampPriceProvider>(client =>
        {
            client.BaseAddress = new Uri(BitstampBaseAddress);
        }).AddStandardResilienceHandler();

        services.AddHttpClient<BitfinexPriceProvider>(client =>
        {
            client.BaseAddress = new Uri(BitfinexBaseAddress);
        }).AddStandardResilienceHandler();

        services.AddScoped<IPriceRepository, EfCorePriceRepository>();
        services.AddScoped<IPriceProvider>(sp => sp.GetRequiredService<BitstampPriceProvider>());
        services.AddScoped<IPriceProvider>(sp => sp.GetRequiredService<BitfinexPriceProvider>());

        return services;
    }
}

/// <summary>
/// Extension methods for application configuration.
/// </summary>
internal static class ApplicationExtensions
{
    private const string SwaggerUrl = "http://localhost:5118/swagger";

    /// <summary>
    /// Initializes the database with migrations and seeding.
    /// </summary>
    internal static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PriceDbContext>();
        await dbContext.Database.MigrateAsync();
        await PriceDbContextSeeder.SeedAsync(dbContext);
    }

    /// <summary>
    /// Opens Swagger UI in the default browser.
    /// </summary>
    internal static void OpenSwaggerInBrowser(this WebApplication app)
    {
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register(() =>
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = SwaggerUrl,
                    UseShellExecute = true
                });
            }
            catch
            {
                // Ignore errors when opening browser
            }
        });
    }
}

