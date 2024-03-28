using System.Diagnostics.CodeAnalysis;

using MemoryPack;

namespace Sharpify.Data;

/// <summary>
/// Represents a filter for a database that provides operations for querying, retrieving, and modifying data.
/// </summary>
/// <typeparam name="T">The type of data stored in the database.</typeparam>
public interface IDatabaseFilter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> where T : IMemoryPackable<T> {
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
	bool TryGetValue(string key, out T value);

	/// <summary>
	/// Gets the values for the specified key from the database.
	/// </summary>
	/// <param name="key">The key to retrieve the value for.</param>
	/// <param name="values">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
	/// <returns><c>true</c> if the value was successfully retrieved; otherwise, <c>false</c>.</returns>
	bool TryGetValues(string key, out T[] values);

	/// <summary>
	/// Gets the value for the specified key from the database using the specified encryption key.
	/// </summary>
	/// <param name="key">The key to retrieve the value for.</param>
	/// <param name="encryptionKey">The encryption key to use.</param>
	/// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
	/// <returns><c>true</c> if the value was successfully retrieved; otherwise, <c>false</c>.</returns>
	bool TryGetValue(string key, string encryptionKey, out T value);

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
	void Upsert(string key, T value, string encryptionKey = "");

	/// <summary>
	/// Upserts multiple values into the database under a single key.
	/// </summary>
	/// <param name="key">The key to upsert the value for.</param>
	/// <param name="values">The value to upsert.</param>
	/// <param name="encryptionKey">The encryption key to use.</param>
	void UpsertMany(string key, T[] values, string encryptionKey = "");

	/// <summary>
	/// Performs an atomic upsert operation on the database. While this key is in use, other threads cannot access its value.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="transform"></param>
	/// <param name="encryptionKey"></param>
	/// <returns>The result of the processor, which if successful, contains the new value for this key</returns>
	/// <remarks>
	/// This method should only be used in specific scenarios where you need to ensure that the processing always happens on the latest value. If <see cref="Result{T}"/> is misused, such as the value is null for success, an exception will be thrown.
	/// </remarks>
	Result<T> AtomicUpsert(string key, Func<T, Result<T>> transform, string encryptionKey = "");

	/// <summary>
	/// Performs an atomic upsert operation on the database. While this key is in use, other threads cannot access its value.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="transform"></param>
	/// <param name="encryptionKey"></param>
	/// <returns>The result of the processor, which if successful, contains the new value for this key</returns>
	/// <remarks>
	/// This method should only be used in specific scenarios where you need to ensure that the processing always happens on the latest value. If <see cref="Result{T}"/> is misused, such as the value is null for success, an exception will be thrown.
	/// </remarks>
	Result<T[]> AtomicUpsertMany(string key, Func<T[], Result<T[]>> transform, string encryptionKey = "");


	/// <summary>
	/// Removes the item with the specified key from the filtered database.
	/// </summary>
	/// <param name="key">The key of the item to remove.</param>
	/// <returns><c>true</c> if the item was successfully removed; otherwise, <c>false</c>.</returns>
	bool Remove(string key);
}