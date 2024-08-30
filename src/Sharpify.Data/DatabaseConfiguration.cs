using MemoryPack;

namespace Sharpify.Data;

/// <summary>
/// Configuration for <see cref="Database"/>
/// </summary>
public record DatabaseConfiguration {
    /// <summary>
    /// The path to which the database file will be saved.
    /// </summary>
    /// <remarks>
    /// Setting path to an empty string "" will create an in-memory database.
    /// </remarks>
    public required string Path { get; init; }

    /// <summary>
    /// Whether the database keys case should be ignored.
    /// </summary>
    /// <remarks>
    /// This impacts performance on reads and deserialization.
    /// </remarks>
    public bool IgnoreCase { get; init; } = false;

    /// <summary>
    /// The encoding to use when serializing and deserializing strings in the database.
    /// </summary>
    public StringEncoding Encoding { get; init; } = StringEncoding.Utf8;

    /// <summary>
    /// Whether to serialize the database automatically when it is updated.
    /// </summary>
    /// <remarks>
    /// This relates to adding, removing, and updating values.
    /// </remarks>
    public bool SerializeOnUpdate { get; init; } = false;

    /// <summary>
    /// Whether to trigger update events when the database is updated.
    /// </summary>
    /// <remarks>
    /// This relates to adding, removing, and updating values.
    /// </remarks>
    public bool TriggerUpdateEvents { get; init; } = false;

    /// <summary>
    /// General encryption key, the entire file will be encrypted with this.
    /// </summary>
    public string EncryptionKey { get; init; } = "";

    /// <summary>
    /// Whether general encryption is enabled.
    /// </summary>
    public bool HasEncryption => EncryptionKey.Length > 0;
}