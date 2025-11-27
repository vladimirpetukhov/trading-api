using BtcPrice.GrpcService;
using BtcPrice.GrpcService.Endpoints;
using BtcPrice.GrpcService.Endpoints.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5118, lo => lo.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
    options.ListenLocalhost(5119, lo => lo.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
});

builder.Services
    .AddPresentation()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.InitializeDatabaseAsync();

app.UseMiddleware<ValidationMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BTC Price Service v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseGrpcWeb();
app.MapGrpcService<PriceGrpcService>();
app.MapGrpcReflectionService();
app.MapPriceEndpoints();

if (app.Environment.IsDevelopment())
{
    app.OpenSwaggerInBrowser();
}

app.Run();
