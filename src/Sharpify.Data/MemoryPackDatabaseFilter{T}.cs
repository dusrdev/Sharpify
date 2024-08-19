using System.Runtime.CompilerServices;

using MemoryPack;

using Sharpify.Collections;

namespace Sharpify.Data;

/// <summary>
/// Provides a light database filter by type.
/// </summary>
/// <remarks>
/// Items that are upserted into the database using the filter, should not be retrieved without the filter as the key is modified.
/// </remarks>
/// <typeparam name="T"></typeparam>
public class MemoryPackDatabaseFilter<T> : IDatabaseFilter<T> where T : IMemoryPackable<T> {
    /// <summary>
    /// The key filter, statically created for the type.
    /// </summary>
    public static readonly string KeyFilter = $"{typeof(T).Name}:";

    /// <summary>
    /// Creates a combined key (filter) for the specified key.
    /// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual string AcquireKey(ReadOnlySpan<char> key) => string.Intern(KeyFilter.Concat(key));

    /// <summary>
    /// The database.
    /// </summary>
	protected readonly Database _database;

    /// <summary>
    /// Creates a new database filter.
    /// </summary>
    /// <param name="database"></param>
	public MemoryPackDatabaseFilter(Database database) {
        _database = database;
    }

    /// <inheritdoc />
    public bool ContainsKey(string key) => _database.ContainsKey(AcquireKey(key));

    /// <inheritdoc />
    public bool TryGetValue(string key, string encryptionKey, out T value) => _database.TryGetValue(AcquireKey(key), encryptionKey, out value);

    /// <inheritdoc />
    public bool TryGetValues(string key, string encryptionKey, out T[] values) => _database.TryGetValues(AcquireKey(key), encryptionKey, out values);

    /// <inheritdoc />
    public RentedBufferWriter<T> TryReadToRentedBuffer(string key, string encryptionKey = "", int reservedCapacity = 0)
        => _database.TryReadToRentedBuffer<T>(AcquireKey(key), encryptionKey, reservedCapacity);

    /// <inheritdoc />
    public bool Upsert(string key,
                       T value,
                       string encryptionKey = "",
                       Func<T, bool>? updateCondition = null)
                       => _database.Upsert(AcquireKey(key), value, encryptionKey, updateCondition);

    /// <inheritdoc />
    public bool UpsertMany(string key,
                           T[] values,
                           string encryptionKey = "",
                           Func<T[], bool>? updateCondition = null)
                           => _database.UpsertMany(AcquireKey(key), values, encryptionKey, updateCondition);

    /// <inheritdoc />
    public bool UpsertMany(string key,
                           ReadOnlySpan<T> values,
                           string encryptionKey = "",
                           Func<T[], bool>? updateCondition = null)
                           => _database.UpsertMany(AcquireKey(key), values, encryptionKey, updateCondition);

    /// <inheritdoc />
    public bool Remove(string key) => _database.Remove(AcquireKey(key));

    /// <inheritdoc />
    public void Remove(Func<string, bool> keySelector) => _database.Remove(keySelector, KeyFilter);

    /// <inheritdoc />
    public void Serialize() => _database.Serialize();

    /// <inheritdoc />
    public ValueTask SerializeAsync(CancellationToken cancellationToken = default) => _database.SerializeAsync(cancellationToken);
}