namespace Sharpify.Data;

/// <summary>
/// Provides an abstraction for creating a readonly serializer
/// </summary>
internal abstract class DatabaseSerializer {
	protected readonly string _path;

    protected DatabaseSerializer(string path) {
        _path = path;
    }

    /// <summary>
    /// Serializes the given dictionary
    /// </summary>
    /// <param name="dict"></param>
    internal abstract void Serialize(Dictionary<string, ReadOnlyMemory<byte>> dict);

    /// <summary>
    /// Serializes the given dictionary asynchronously
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal abstract ValueTask SerializeAsync(Dictionary<string, ReadOnlyMemory<byte>> dict, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deserializes the path to a dictionary
	/// </summary>
	internal abstract Dictionary<string, ReadOnlyMemory<byte>> Deserialize();

	/// <summary>
	/// Deserializes the path to a dictionary asynchronously
	/// </summary>
	/// <param name="cancellationToken"></param>
	internal abstract ValueTask<Dictionary<string, ReadOnlyMemory<byte>>> DeserializeAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a serializer based on the given configuration
	/// </summary>
	/// <param name="configuration"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	internal static DatabaseSerializer Create(DatabaseConfiguration configuration) {
		return configuration switch {
			{HasEncryption: true, IgnoreCase: true} => new IgnoreCaseEncryptedSerializer(configuration.Path, configuration.EncryptionKey),
			{HasEncryption: true, IgnoreCase: false} => new EncryptedSerializer(configuration.Path, configuration.EncryptionKey),
			{HasEncryption: false, IgnoreCase: true} => new IgnoreCaseSerializer(configuration.Path),
			{HasEncryption: false, IgnoreCase: false} => new Serializer(configuration.Path),
			_ => throw new ArgumentException("Invalid configuration")
		};
	}
}