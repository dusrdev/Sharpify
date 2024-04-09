using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

using MemoryPack;

namespace Sharpify.Data;

/// <summary>
/// A high performance database that stores String:byte[] pairs.
/// </summary>
/// <remarks>
/// Do not create this class directly or by using an activator, the factory methods are required for proper initializations using different abstractions.
/// </remarks>
public sealed partial class Database : IDisposable {
    private readonly Dictionary<string, byte[]> _data;

    private readonly ConcurrentQueue<KeyValuePair<string, byte[]>> _queue = new();

    private volatile bool _disposed;

    private readonly ReaderWriterLockSlim _lock = new();
    private readonly DatabaseSerializer _serializer;
    private volatile int _estimatedSize;

    private const int BufferMultiple = 4096;
    private const int ReservedBufferSize = 256;

    /// <summary>
    /// Overestimated size of the database.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private int GetOverestimatedSize() {
        return (int)Math.Ceiling((_estimatedSize + ReservedBufferSize) / (double)BufferMultiple) * BufferMultiple;
    }


    /// <summary>
    /// Holds the configuration for this database.
    /// </summary>
    public readonly DatabaseConfiguration Config;

    /// <summary>
    /// Triggered when there is a change in the database.
    /// </summary>
    public event DataChangedEventHandler? DataChanged;

    /// <summary>
    /// Represents the method that will handle the data changed event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An instance of the DataChangedEventArgs class that contains the event data.</param>
    public delegate void DataChangedEventHandler(object sender, DataChangedEventArgs e);

    private void InvokeDataEvent(DataChangedEventArgs e) {
        DataChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Creates a high performance database that stores string-byte[] pairs.
    /// </summary>
    public static Database CreateOrLoad(DatabaseConfiguration config) {
        DatabaseSerializer serializer = DatabaseSerializer.Create(config);

        if (!File.Exists(config.Path)) {
            return config.IgnoreCase
                ? new Database(new(StringComparer.OrdinalIgnoreCase), config, serializer, 0)
                : new Database(new Dictionary<string, byte[]>(), config, serializer, 0);
        }

        var estimatedSize = Extensions.GetFileSize(config.Path);

        Dictionary<string, byte[]> dict = serializer.Deserialize(estimatedSize);

        return new Database(dict, config, serializer, estimatedSize);
    }

    /// <summary>
    /// Creates asynchronously a high performance database that stores string-byte[] pairs.
    /// </summary>
    public static async ValueTask<Database> CreateOrLoadAsync(DatabaseConfiguration config, CancellationToken token = default) {
        DatabaseSerializer serializer = DatabaseSerializer.Create(config);

        if (!File.Exists(config.Path)) {
            return config.IgnoreCase
                ? new Database(new(StringComparer.OrdinalIgnoreCase), config, serializer, 0)
                : new Database(new Dictionary<string, byte[]>(), config, serializer, 0);
        }

        var estimatedSize = Extensions.GetFileSize(config.Path);

        Dictionary<string, byte[]> dict = await serializer.DeserializeAsync(estimatedSize, token);

        return new Database(dict, config, serializer, estimatedSize);
    }

    private Database(Dictionary<string, byte[]> data, DatabaseConfiguration config, DatabaseSerializer serializer, int estimatedSize) {
        _data = data;
        Config = config;
        _serializer = serializer;
        Interlocked.Exchange(ref _estimatedSize, estimatedSize);
    }

    /// <summary>
    /// Returns the amount of entries in the database.
    /// </summary>
    public int Count => _data.Count;

    /// <summary>
    /// Returns a <see cref="DatabaseFilter{T}"/> that can be used to filter the database by type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IDatabaseFilter<T> FilterByType<T>() where T : IMemoryPackable<T> => new DatabaseFilter<T>(this);

    /// <summary>
    /// Returns an immutable copy of the keys in the inner dictionary
    /// </summary>
    public IReadOnlyCollection<string> GetKeys() {
        try {
            _lock.EnterReadLock();
            return _data.Keys;
        } finally {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Frees the resources used by the database.
    /// </summary>
    public void Dispose() {
        if (_disposed) {
            return;
        }
        _lock.Dispose();
        _disposed = true;
    }
}