using Sharpify.Collections;

namespace Sharpify.Data;

/// <summary>
/// Represents a filter for a database that provides operations for querying, retrieving, and modifying data.
/// </summary>
/// <typeparam name="T">The type of data stored in the database.</typeparam>
public interface IDatabaseFilter<T> {
	/// <summary>
	/// Checks if the filtered database contains the specified key.
	/// </summary>
	/// <param name="key">The key to check.</param>
	/// <returns><c>true</c> if the database contains the key; otherwise, <c>false</c>.</returns>
	bool ContainsKey(string key);

	/// <summary>
	/// Gets the value for the specified key from the database.
	/// </summary>
	/// <param name="key">The key to retrieve the value for.</param>
	/// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
	/// <returns><c>true</c> if the value was successfully retrieved; otherwise, <c>false</c>.</returns>
	bool TryGetValue(string key, out T value) => TryGetValue(key, "", out value);

	/// <summary>
	/// Gets the value for the specified key from the database using the specified encryption key.
	/// </summary>
	/// <param name="key">The key to retrieve the value for.</param>
	/// <param name="encryptionKey">The encryption key to use.</param>
	/// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
	/// <returns><c>true</c> if the value was successfully retrieved; otherwise, <c>false</c>.</returns>
	bool TryGetValue(string key, string encryptionKey, out T value);

	/// <summary>
    /// Tries to get the values for the <paramref name="key"/> and write it to a <see cref="RentedBufferWriter{T}"/>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="reservedCapacity">Reserved capacity after the values, useful to write additional data</param>
    /// <returns>
    /// A rented buffer writer containing the values if they were found, otherwise a disabled buffer writer (can be checked with <see cref="RentedBufferWriter{T}.IsDisabled"/>)
    /// </returns>
	RentedBufferWriter<T> TryReadToRentedBuffer(string key, int reservedCapacity = 0) => TryReadToRentedBuffer(key, "", reservedCapacity);

	/// <summary>
    /// Tries to get the values for the <paramref name="key"/> and write it to a <see cref="RentedBufferWriter{T}"/>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="encryptionKey"></param>
    /// <param name="reservedCapacity">Reserved capacity after the values, useful to write additional data</param>
    /// <returns>
    /// A rented buffer writer containing the values if they were found, otherwise a disabled buffer writer (can be checked with <see cref="RentedBufferWriter{T}.IsDisabled"/>)
    /// </returns>
	RentedBufferWriter<T> TryReadToRentedBuffer(string key, string encryptionKey = "", int reservedCapacity = 0);

	/// <summary>
	/// Gets the values for the specified key from the database.
	/// </summary>
	/// <param name="key">The key to retrieve the value for.</param>
	/// <param name="values">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
	/// <returns><c>true</c> if the value was successfully retrieved; otherwise, <c>false</c>.</returns>
	bool TryGetValues(string key, out T[] values) => TryGetValues(key, "", out values);

	/// <summary>
	/// Gets the values for the specified key from the database using the specified encryption key.
	/// </summary>
	/// <param name="key">The key to retrieve the value for.</param>
	/// <param name="encryptionKey">The encryption key to use.</param>
	/// <param name="values">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
	/// <returns><c>true</c> if the value was successfully retrieved; otherwise, <c>false</c>.</returns>
	bool TryGetValues(string key, string encryptionKey, out T[] values);

	/// <summary>
	/// Upserts the value into the database.
	/// </summary>
	/// <param name="key">The key to upsert the value for.</param>
	/// <param name="value">The value to upsert.</param>
	/// <param name="encryptionKey">The encryption key to use.</param>
	/// <remarks>
	/// Null values are disallowed and will cause an exception to be thrown.
	/// </remarks>
	void Upsert(string key, T value, string encryptionKey = "");

	/// <summary>
	/// Upserts multiple values into the database under a single key.
	/// </summary>
	/// <param name="key">The key to upsert the values for.</param>
	/// <param name="values">The values to upsert.</param>
	/// <param name="encryptionKey">The encryption key to use.</param>
	/// <remarks>
	/// Null values are disallowed and will cause an exception to be thrown.
	/// </remarks>
	void UpsertMany(string key, T[] values, string encryptionKey = "");

	/// <summary>
	/// Upserts multiple values into the database under a single key.
	/// </summary>
	/// <param name="key">The key to upsert the values for.</param>
	/// <param name="values">The values to upsert.</param>
	/// <param name="encryptionKey">The encryption key to use.</param>
	void UpsertMany(string key, ReadOnlySpan<T> values, string encryptionKey = "");

	/// <summary>
	/// Removes the item with the specified key from the filtered database.
	/// </summary>
	/// <param name="key">The key of the item to remove.</param>
	/// <returns><c>true</c> if the item was successfully removed; otherwise, <c>false</c>.</returns>
	bool Remove(string key);

	/// <summary>
    /// Removes all keys that match the <paramref name="keySelector"/>, while applying the key filtering
    /// </summary>
    /// <param name="keySelector"></param>
    /// <remarks>
    /// <para>
    /// This method is thread-safe and will lock the database while removing the keys.
    /// </para>
    /// <para>
    /// If TriggerUpdateEvents is enabled, this method will trigger a <see cref="DataChangedEventArgs"/> event for each key removed.
    /// </para>
    /// </remarks>
	void Remove(Func<string, bool> keySelector);

	/// <summary>
	/// Serializes the database.
	/// </summary>
	public void Serialize();

	/// <summary>
	/// Serializes the database asynchronously.
	/// </summary>
	public ValueTask SerializeAsync(CancellationToken cancellationToken = default);
}