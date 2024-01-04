namespace Sharpify.Data;

/// <summary>
/// Configuration for generic databases
/// </summary>
/// <typeparam name="T">The type for the values</typeparam>
public record DatabaseConfiguration<T> : DatabaseConfiguration {
    /// <summary>
    /// Serialization function for <typeparamref name="T"/> into byte[].
    /// </summary>
    public required Func<T, byte[]> ToByteArray { get; init; }

    /// <summary>
    /// Deserialization function from byte[] into <typeparamref name="T"/>.
    /// </summary>
    public required Func<byte[], T> ToT { get; init; }
}