namespace Sharpify.Data;

/// <summary>
/// Configuration for the polymorphic database
/// </summary>
public record DatabaseConfiguration {
    /// <summary>
    /// The path to which the database file will be saved.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Database options
    /// </summary>
    public DatabaseOptions Options { get; init; }

    /// <summary>
    /// General encryption key, the entire file will be encrypted with this.
    /// </summary>
    public string EncryptionKey { get; init; } = "";
}