using System.Collections.Concurrent;

using MemoryPack;

using Sharpify.Data.Serializers;

namespace Sharpify.Data;

/// <summary>
/// A high performance database that stores String:byte[] pairs.
/// </summary>
/// <remarks>
/// Do not create this class directly or by using an activator, the factory methods are required for proper initializations using different abstractions.
/// </remarks>
public sealed partial class Database : IDisposable {
    /// <summary>
    /// The unique identifier of the database.
    /// </summary>
    public readonly Guid Guid = Guid.NewGuid();

    private readonly ConcurrentDictionary<string, byte[]?> _data;

    private readonly ConcurrentQueue<KeyValuePair<string, byte[]>> _queue = new();

    private volatile bool _disposed;

    // The updates count increments every time a value is updated, added or removed.
    private long _updatesCount = 0;

    // The serialization reference is checking against the updates to reduce redundant serialization.
    private long _serializationReference = 0;

#if NET9_0_OR_GREATER
    private readonly Lock _sLock = new();
#else
    private readonly object _sLock = new();
#endif
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly AbstractSerializer _serializer;
    private volatile int _estimatedSize;

    private const int BufferMultiple = 4096;
    private const int ReservedBufferSize = 256;

    private readonly bool _isInMemory;

    /// <summary>
    /// Overestimated size of the database.
    /// </summary>
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
        AbstractSerializer serializer = AbstractSerializer.Create(config);

        if (!File.Exists(config.Path)) {
            return config.IgnoreCase
                ? new Database(new ConcurrentDictionary<string, byte[]?>(StringComparer.OrdinalIgnoreCase), config, serializer, 0)
                : new Database(new ConcurrentDictionary<string, byte[]?>(), config, serializer, 0);
        }

        int estimatedSize = Helper.GetFileSize(config.Path);

        ConcurrentDictionary<string, byte[]?> dict = serializer.Deserialize(estimatedSize);

        return new Database(dict, config, serializer, estimatedSize);
    }

    /// <summary>
    /// Creates asynchronously a high performance database that stores string-byte[] pairs.
    /// </summary>
    public static async ValueTask<Database> CreateOrLoadAsync(DatabaseConfiguration config, CancellationToken token = default) {
        AbstractSerializer serializer = AbstractSerializer.Create(config);

        if (!File.Exists(config.Path)) {
            return config.IgnoreCase
                ? new Database(new ConcurrentDictionary<string, byte[]?>(StringComparer.OrdinalIgnoreCase), config, serializer, 0)
                : new Database(new ConcurrentDictionary<string, byte[]?>(), config, serializer, 0);
        }

        int estimatedSize = Helper.GetFileSize(config.Path);

        ConcurrentDictionary<string, byte[]?> dict = await serializer.DeserializeAsync(estimatedSize, token);

        return new Database(dict, config, serializer, estimatedSize);
    }

    private Database(ConcurrentDictionary<string, byte[]?> data, DatabaseConfiguration config, AbstractSerializer serializer, int estimatedSize) {
        _data = data;
        Config = config;
        _serializer = serializer;
        _isInMemory = config.Path.Length == 0;
        Interlocked.Exchange(ref _estimatedSize, estimatedSize);
    }

    static Database() {
        MemoryPackFormatterProvider.RegisterDictionary<ConcurrentDictionary<string, byte[]?>, string, byte[]>();
    }

    /// <summary>
    /// Returns the amount of entries in the database.
    /// </summary>
    public int Count => _data.Count;

    /// <summary>
    /// Returns a <see cref="MemoryPackDatabaseFilter{T}"/> that can be used to filter the database by type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IDatabaseFilter<T> CreateMemoryPackFilter<T>() where T : IMemoryPackable<T> => new MemoryPackDatabaseFilter<T>(this);

    /// <summary>
    /// Returns a <see cref="MemoryPackDatabaseFilter{T}"/> that can be used to filter the database by type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IDatabaseFilter<T> CreateFlexibleFilter<T>() where T : IFilterable<T> => new FlexibleDatabaseFilter<T>(this);

    /// <summary>
    /// Returns an immutable copy of the keys in the inner dictionary
    /// </summary>
    public IReadOnlyCollection<string> GetKeys() {
        return (IReadOnlyCollection<string>)_data.Keys;
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