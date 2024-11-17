using System.Collections.Concurrent;

using MemoryPack;

namespace Sharpify.Data.Serializers;

/// <summary>
/// Provides an abstraction for creating a readonly serializer
/// </summary>
internal abstract class AbstractSerializer {
    protected readonly string _path;
    internal readonly MemoryPackSerializerOptions SerializerOptions;

    protected AbstractSerializer(string path, StringEncoding encoding = StringEncoding.Utf8) {
        _path = path;
        SerializerOptions = encoding switch {
            StringEncoding.Utf8 => MemoryPackSerializerOptions.Utf8,
            StringEncoding.Utf16 => MemoryPackSerializerOptions.Utf16,
            _ => MemoryPackSerializerOptions.Default
        };
    }

    /// <summary>
    /// Serializes the given dictionary
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="estimatedSize"></param>
    internal abstract void Serialize(ConcurrentDictionary<string, byte[]?> dict, int estimatedSize);

    /// <summary>
    /// Serializes the given dictionary asynchronously
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal abstract ValueTask SerializeAsync(ConcurrentDictionary<string, byte[]?> dict, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deserializes the path to a dictionary
    /// </summary>
    /// <param name="estimatedSize"></param>
    internal abstract ConcurrentDictionary<string, byte[]?> Deserialize(int estimatedSize);

    /// <summary>
    /// Deserializes the path to a dictionary asynchronously
    /// </summary>
    /// <param name="estimatedSize"></param>
    /// <param name="cancellationToken"></param>
    internal abstract ValueTask<ConcurrentDictionary<string, byte[]?>> DeserializeAsync(int estimatedSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a serializer based on the given configuration
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    internal static AbstractSerializer Create(DatabaseConfiguration configuration) {
        return configuration switch {
            { Path: "", IgnoreCase: false } => new DisabledSerializer(configuration.Path, configuration.Encoding),
            { Path: "", IgnoreCase: true } => new DisabledIgnoreCaseSerializer(configuration.Path, configuration.Encoding),
            { HasEncryption: true, IgnoreCase: true } => new IgnoreCaseEncryptedSerializer(configuration.Path, configuration.EncryptionKey),
            { HasEncryption: true, IgnoreCase: false } => new EncryptedSerializer(configuration.Path, configuration.EncryptionKey),
            { HasEncryption: false, IgnoreCase: true } => new IgnoreCaseSerializer(configuration.Path),
            { HasEncryption: false, IgnoreCase: false } => new Serializer(configuration.Path),
            _ => throw new ArgumentException("Invalid configuration")
        };
    }
}