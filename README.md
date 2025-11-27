# BTC Price gRPC Service

## Overview

This service exposes a gRPC API that aggregates BTC/USD prices per hour from Bitstamp and Bitfinex, caches the aggregated result in SQLite, and allows retrieval by hour or by time range.

## Requirements

- .NET SDK 9.0 or later

## Build

```bash
dotnet build
```

## Run

```bash
dotnet run --project BtcPrice.GrpcService/BtcPrice.GrpcService.csproj
```

The default HTTP/2 endpoint is `https://localhost:7013` (or `http://localhost:5116` for plaintext gRPC).

## REST API & Swagger UI

In development mode, Swagger documentation is available at:

```
http://localhost:5116/swagger
```

This provides an interactive interface to explore and test the REST API endpoints.

### REST Endpoints

The service exposes REST endpoints alongside gRPC for easier integration:

- **GET** `/api/prices/aggregated?timestamp=2024-01-01T10:00:00Z` - Get aggregated price for a specific hour
- **GET** `/api/prices/history?from=2024-01-01T00:00:00Z&to=2024-01-01T23:00:00Z` - Get price history for a time range

## gRPC API

Service: `btcprice.PriceService`

### GetAggregatedPrice

Returns the aggregated BTC/USD price for a specific hour.

- Request: `GetAggregatedPriceRequest { google.protobuf.Timestamp timestamp }`
- Response: `GetAggregatedPriceResponse { PricePoint price }`

If the price for the requested hour is already cached in SQLite, it is returned from the database. Otherwise the service calls Bitstamp and Bitfinex, aggregates the close prices that are available, stores the result, and returns it.

### GetPriceHistory

Returns all persisted hourly prices in a time range.

- Request: `GetPriceHistoryRequest { google.protobuf.Timestamp from, google.protobuf.Timestamp to }`
- Response: `GetPriceHistoryResponse { repeated PricePoint prices }`

Both `from` and `to` are interpreted as UTC and rounded down to the start of the hour. The range is inclusive.

### PricePoint

- `google.protobuf.Timestamp timestamp` (UTC, hour precision)
- `double price`

## Aggregation

The aggregation uses the average of the close prices returned by Bitstamp and Bitfinex for the requested hour. Providers that fail or return no data are ignored. If no provider returns a price, the service returns a gRPC `NOT_FOUND` error.

## External APIs

- **Bitstamp**: `https://www.bitstamp.net/api/v2/ohlc/btcusd/?step=3600&limit=1&start={epochSeconds}`
- **Bitfinex**: `https://api-pub.bitfinex.com/v2/candles/trade:1h:tBTCUSD/hist?start={epochMs}&end={epochMs}&limit=1`

## SQLite

The service uses a single SQLite database file `prices.db` in the working directory.

Schema:

```sql
CREATE TABLE IF NOT EXISTS Prices (
    Timestamp TEXT PRIMARY KEY,
    Price REAL NOT NULL
);
```

## Example REST API calls

### GetAggregatedPrice

```bash
curl "http://localhost:5116/api/prices/aggregated?timestamp=2024-01-01T10:00:00Z"
```

Response:
```json
{
  "timestamp": "2024-01-01T10:00:00Z",
  "price": 42500.50
}
```

### GetPriceHistory

```bash
curl "http://localhost:5116/api/prices/history?from=2024-01-01T00:00:00Z&to=2024-01-01T23:00:00Z"
```

Response:
```json
[
  {
    "timestamp": "2024-01-01T00:00:00Z",
    "price": 42100.00
  },
  {
    "timestamp": "2024-01-01T01:00:00Z",
    "price": 42250.75
  }
]
```

## Example gRPC calls

### List available services

```bash
grpcurl -plaintext localhost:5116 list
```

### GetAggregatedPrice (gRPC)

```bash
grpcurl -d '{"timestamp": "2024-01-01T10:00:00Z"}' \
  -plaintext localhost:5116 btcprice.PriceService.GetAggregatedPrice
```

### GetPriceHistory (gRPC)

```bash
grpcurl -d '{"from": "2024-01-01T00:00:00Z", "to": "2024-01-01T23:00:00Z"}' \
  -plaintext localhost:5116 btcprice.PriceService.GetPriceHistory
```

Note: gRPC Reflection is enabled, so you don't need to specify the proto file with `-proto` flag.

Adjust the host and port if your Kestrel configuration uses different values.


## Assignment Requirements Mapping

This section summarizes the original assignment requirements and where they are implemented in the codebase.

1. **Aggregated BTC price at a specific hour**
   - REST: `GET /api/prices/aggregated?timestamp=...` in `Endpoints/PriceEndpoints.cs`.
   - gRPC: `GetAggregatedPrice` in `Endpoints/PriceGrpcService.cs`.
   - Business logic: `Application/AggregatedPriceService.GetAggregatedPriceAsync`.

2. **Serve from datastore if available; otherwise fetch, aggregate, persist & return**
   - Implemented in `Application/AggregatedPriceService`:
     - Reads from `IPriceRepository.GetAsync` (`Infrastructure/EfCorePriceRepository.cs`).
     - If missing, calls all `IPriceProvider` implementations (`BitstampPriceProvider`, `BitfinexPriceProvider`), averages the close prices, saves via `IPriceRepository.SaveAsync`, and returns the result.

3. **Endpoint to fetch persisted prices for a time range**
   - REST: `GET /api/prices/history?from=...&to=...` in `Endpoints/PriceEndpoints.cs`.
   - gRPC: `GetPriceHistory` in `Endpoints/PriceGrpcService.cs`.
   - Data access: `IPriceRepository.GetRangeAsync` (`Infrastructure/EfCorePriceRepository.cs`).

4. **Endpoints exposed as GRPC or RESTful API**
   - REST minimal APIs: `Endpoints/PriceEndpoints.cs`.
   - gRPC service and proto contract: `Endpoints/PriceGrpcService.cs`, `Protos/price.proto`.

5. **Implementation using .NET 6 or later**
   - Project targets **.NET 9.0**, which satisfies the "NET 6 or later" requirement.

6. **Datastore option (in-memory / SQLite / DB in Docker)**
   - This implementation uses **SQLite** with EF Core:
     - Entity and DbContext: `Infrastructure/Data/Price.cs`, `Infrastructure/Data/PriceDbContext.cs`.
     - Migrations and automatic initialization: `ServiceCollectionExtensions.InitializeDatabaseAsync`.
     - Seeding: `Infrastructure/Data/PriceDbContextSeeder.cs`.

7. **All prices handled as floating-point types**
   - All prices are `decimal`:
     - Domain: `Domain/PricePoint.Price`.
     - Persistence: `Infrastructure/Data/Price.PriceValue`.

8. **Hour-accuracy for time-points (no minutes/seconds)**
   - Enforced via `Domain/PricePoint.NormalizeToHourUtc`, used consistently in:
     - `AggregatedPriceService`.
     - `BitstampPriceProvider` and `BitfinexPriceProvider`.
     - `EfCorePriceRepository` queries.

9. **Future extensibility (more sources or different aggregation formula)**
   - New sources: implement `Domain/IPriceProvider` and register in `ServiceCollectionExtensions.AddInfrastructure`.
   - Aggregation formula: centralized in `Application/AggregatedPriceService` so the computation can be changed in one place.

10. **Installation and running instructions in README**
    - Provided at the top of this file:
      - Prerequisites in the **Requirements** section.
      - Build command in the **Build** section.
      - Run command in the **Run** section.
