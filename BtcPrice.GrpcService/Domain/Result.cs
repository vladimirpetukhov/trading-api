namespace BtcPrice.GrpcService.Domain;

/// <summary>
/// Represents the result of an operation.
/// </summary>
public abstract record Result
{
    /// <summary>
    /// Represents a successful operation result.
    /// </summary>
    public sealed record Success(object? Data) : Result;

    /// <summary>
    /// Represents a failed operation result.
    /// </summary>
    public sealed record Failure(string Message, int StatusCode = 400) : Result;

    /// <summary>
    /// Represents a not found result.
    /// </summary>
    public sealed record NotFound(string Message = "Resource not found") : Result;
}

/// <summary>
/// Represents the result of an operation with a typed data payload.
/// </summary>
/// <typeparam name="T">The type of data in the result.</typeparam>
public abstract record Result<T>
{
    /// <summary>
    /// Represents a successful operation result with data.
    /// </summary>
    public sealed record Success(T Data) : Result<T>;

    /// <summary>
    /// Represents a failed operation result.
    /// </summary>
    public sealed record Failure(string Message, int StatusCode = 400) : Result<T>;

    /// <summary>
    /// Represents a not found result.
    /// </summary>
    public sealed record NotFound(string Message = "Resource not found") : Result<T>;
}

