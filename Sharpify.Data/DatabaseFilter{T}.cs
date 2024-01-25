using System.Runtime.CompilerServices;

using MemoryPack;

namespace Sharpify.Data;

/// <summary>
/// Provides a light database filter by type.
/// </summary>
/// <remarks>
/// Items that are upserted into the database using the filter, should not be retrieved without the filter as the key is modified.
/// </remarks>
/// <typeparam name="T"></typeparam>
public class DatabaseFilter<T> : IDatabaseFilter<T> where T : IMemoryPackable<T> {
    /// <summary>
    /// The name of the type.
    /// </summary>
	protected static readonly string TName = typeof(T).Name;

    /// <summary>
    /// Creates a combined key (filter) for the specified key.
    /// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual string CreateKey(ReadOnlySpan<char> key) => string.Concat(TName, ":", key);

    /// <summary>
    /// The database.
    /// </summary>
	protected readonly Database _database;

    /// <summary>
    /// Creates a new database filter.
    /// </summary>
    /// <param name="database"></param>
	public DatabaseFilter(Database database) {
		_database = database;
	}

/// <inheritdoc />
	public bool ContainsKey(string key) => _database.ContainsKey(CreateKey(key));

/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(string key, out T value) => TryGetValue(key, "", out value);

/// <inheritdoc />
    public bool TryGetValue(string key, string encryptionKey, out T value) => _database.TryGetValue(CreateKey(key), encryptionKey, out value);

/// <inheritdoc />
    public void Upsert(string key, T value, string encryptionKey = "") => _database.Upsert(CreateKey(key), value, encryptionKey);

/// <inheritdoc />
    public bool Remove(string key) => _database.Remove(CreateKey(key));
}