using System.Runtime.CompilerServices;

namespace Sharpify.Data;

/// <summary>
/// Provides a light database filter by type.
/// </summary>
/// <remarks>
/// Items that are upserted into the database using the filter, should not be retrieved without the filter as the key is modified.
/// </remarks>
/// <typeparam name="T"></typeparam>
public class FlexibleDatabaseFilter<T> : IDatabaseFilter<T> where T : IFilterable<T> {
    /// <summary>
    /// The name of the type.
    /// </summary>
	protected static readonly string TName = typeof(T).Name;

    /// <summary>
    /// The key filter, statically created for the type.
    /// </summary>
    public static readonly string KeyFilter = $"{TName}:";

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
	public FlexibleDatabaseFilter(Database database) {
        _database = database;
    }

    /// <inheritdoc />
    public bool ContainsKey(string key) => _database.ContainsKey(AcquireKey(key));

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(string key, out T value) => TryGetValue(key, "", out value);

    /// <inheritdoc />
    public bool TryGetValue(string key, string encryptionKey, out T value) {
        if (!_database.TryGetValue(AcquireKey(key), encryptionKey, out var data)) {
            value = default!;
            return false;
        }
        value = T.Deserialize(data)!;
        return true;
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValues(string key, out T[] values) => TryGetValues(key, "", out values);

    /// <inheritdoc />
    public bool TryGetValues(string key, string encryptionKey, out T[] values) {
        if (!_database.TryGetValue(AcquireKey(key), encryptionKey, out var data)) {
            values = default!;
            return false;
        }
        values = T.DeserializeMany(data)!;
        return true;
    }

    /// <inheritdoc />
    public void Upsert(string key, T value, string encryptionKey = "") {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        _database.Upsert(AcquireKey(key), T.Serialize(value)!, encryptionKey);
    }



    /// <inheritdoc />
    public void UpsertMany(string key, T[] values, string encryptionKey = "") {
       ArgumentNullException.ThrowIfNull(values, nameof(values));
        _database.Upsert(AcquireKey(key), T.SerializeMany(values)!, encryptionKey);
    }

    /// <inheritdoc />
    public bool Remove(string key) => _database.Remove(AcquireKey(key));

    /// <inheritdoc />
    public void Remove(Func<string, bool> keySelector) => _database.Remove(keySelector, KeyFilter);

    /// <inheritdoc />
    public void Serialize() => _database.Serialize();

    /// <inheritdoc />
    public ValueTask SerializeAsync(CancellationToken cancellationToken = default) => _database.SerializeAsync(cancellationToken);
}