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
public readonly struct DatabaseFilter<T> where T : IMemoryPackable<T> {
	private static readonly string TName = typeof(T).Name;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static string CreateKey(ReadOnlySpan<char> key) => string.Concat(TName, key);

	private readonly Database _database;

	internal DatabaseFilter(Database database) {
		_database = database;
	}

	/// <summary>
	/// Checks if the filtered database contains the specified key.
	/// </summary>
	public bool ContainsKey(string key) => _database.ContainsKey(CreateKey(key));

	/// <summary>
    /// Gets the specified value from the database.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns>false if the value doesn't exist, true if it does</returns>
	/// <remarks>This method assumes no encryption was used on the value, for encrypted values use <see cref="TryGetValue(string,string,out T)"/> </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(string key, out T value) => TryGetValue(key, "", out value);

    /// <summary>
    /// Gets the specified value from the database.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="encryptionKey"></param>
    /// <param name="value"></param>
    /// <returns>false if the value doesn't exist, true if it does</returns>
    public bool TryGetValue(string key, string encryptionKey, out T value) {
		var val = _database.Get<T>(CreateKey(key), encryptionKey);
		if (val is null) {
			value = default!;
			return false;
		}
		value = val;
		return true;
	}

	/// <summary>
	/// Upserts the value into the database.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <param name="encryptionKey"></param>
	public void Upsert(string key, T value, string encryptionKey = "") {
		_database.Upsert(CreateKey(key), value, encryptionKey);
	}

	/// <summary>
	/// Removes the item with specified key from the filtered database.
	/// </summary>
	public bool Remove(string key) => _database.Remove(CreateKey(key));
}