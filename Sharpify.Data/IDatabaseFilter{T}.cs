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
	bool TryGetValue(string key, out T? value);

	/// <summary>
	/// Gets the values for the specified key from the database.
	/// </summary>
	/// <param name="key">The key to retrieve the value for.</param>
	/// <param name="values">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
	/// <returns><c>true</c> if the value was successfully retrieved; otherwise, <c>false</c>.</returns>
	bool TryGetValues(string key, out T[]? values);

	/// <summary>
	/// Gets the value for the specified key from the database using the specified encryption key.
	/// </summary>
	/// <param name="key">The key to retrieve the value for.</param>
	/// <param name="encryptionKey">The encryption key to use.</param>
	/// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
	/// <returns><c>true</c> if the value was successfully retrieved; otherwise, <c>false</c>.</returns>
	bool TryGetValue(string key, string encryptionKey, out T? value);

	/// <summary>
	/// Gets the values for the specified key from the database using the specified encryption key.
	/// </summary>
	/// <param name="key">The key to retrieve the value for.</param>
	/// <param name="encryptionKey">The encryption key to use.</param>
	/// <param name="values">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
	/// <returns><c>true</c> if the value was successfully retrieved; otherwise, <c>false</c>.</returns>
	bool TryGetValues(string key, string encryptionKey, out T[]? values);

	/// <summary>
	/// Upserts the value into the database.
	/// </summary>
	/// <param name="key">The key to upsert the value for.</param>
	/// <param name="value">The value to upsert.</param>
	/// <param name="encryptionKey">The encryption key to use.</param>
	void Upsert(string key, T? value, string encryptionKey = "");

	/// <summary>
	/// Upserts multiple values into the database under a single key.
	/// </summary>
	/// <param name="key">The key to upsert the value for.</param>
	/// <param name="values">The value to upsert.</param>
	/// <param name="encryptionKey">The encryption key to use.</param>
	void UpsertMany(string key, T[]? values, string encryptionKey = "");

	/// <summary>
	/// Removes the item with the specified key from the filtered database.
	/// </summary>
	/// <param name="key">The key of the item to remove.</param>
	/// <returns><c>true</c> if the item was successfully removed; otherwise, <c>false</c>.</returns>
	bool Remove(string key);

	/// <summary>
	/// Serializes the database.
	/// </summary>
	public void Serialize();

	/// <summary>
	/// Serializes the database asynchronously.
	/// </summary>
	public ValueTask SerializeAsync(CancellationToken cancellationToken = default);
}