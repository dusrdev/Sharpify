using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;

using KVP = (string Key, string Value);

namespace Sharpify.Collections;

/// <summary>
/// Provides a thread-safe dictionary that can be efficiently persisted.
/// </summary>
public abstract class PersistentDictionary {
    /// <summary>
    /// A thread-safe dictionary that stores string keys and values.
    /// </summary>
    protected Dictionary<string, string> _dict = [];

    private readonly ConcurrentQueue<KVP> _queue = new();

    private volatile int _updatingConcurrently;

    /// <summary>
    /// Gets the number of key-value pairs contained in the PersistentDictionary.
    /// </summary>
    public int Count => _dict.Count;

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <returns>The value associated with the specified key, or null if the key is not found.</returns>
    protected virtual string? GetValueByKey(string key) {
        if (Count is 0) {
            return null;
        }
        ref var value = ref _dict.GetValueRefOrNullRef(key);
        return Unsafe.IsNullRef(ref value) ? null : value;
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    public virtual string? this[string key] => GetValueByKey(key);

    /// <summary>
    /// Gets the value associated with the specified key, or creates a new key-value pair if the key does not exist.
    /// </summary>
    /// <param name="key">The key of the element to get or create.</param>
    /// <param name="default">The default value to use if the key does not exist.</param>
    /// <returns>The value associated with the specified key, or the default value if the key does not exist.</returns>
    public virtual async ValueTask<string> GetOrCreateAsync(string key, string @default) {
        if (string.IsNullOrWhiteSpace(key)) {
            throw new ArgumentNullException(nameof(key));
        }

        var value = GetValueByKey(key);
        if (value is not null) {
            return value;
        }

        await UpsertAsync(key, @default);
        return @default;
    }

    /// <summary>
    /// Gets the value associated with the specified key, or creates a new value if the key does not exist.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the value.</param>
    /// <param name="default">The default value to create if the key does not exist.</param>
    /// <returns>The value associated with the key, or the created default value.</returns>
    public async ValueTask<T> GetOrCreateAsync<T>(string key, T @default) where T : struct, IParsable<T> {
        var value = await GetOrCreateAsync(key, @default.ToString() ?? "");
        return T.Parse(value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Sets the specified key and value in the dictionary.
    /// </summary>
    /// <param name="key">The key to set.</param>
    /// <param name="value">The value to set.</param>
    protected virtual void SetKeyAndValue(string key, string value) => _dict![key] = value;

    /// <summary>
    /// Inserts or updates a key-value pair in the dictionary and serializes.
    /// </summary>
    /// <param name="key">The key to insert or update.</param>
    /// <param name="value">The value to insert or update.</param>
    public virtual async ValueTask UpsertAsync(string key, string value) {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) {
            return;
        }

        // Reduce the number of times we need to serialize the dictionary.

        _queue.Enqueue((key, value));

        // If it was 0, the method wasn't in use (no concurrent writes)
        // So we insert the KVPs and serialize the dictionary.
        // If it is in use we just insert the KVPs and the previous concurrent call will serialize the dictionary.
        if (0 == Interlocked.Exchange(ref _updatingConcurrently, 1)) {
            while (_queue.TryDequeue(out var item)) {
                SetKeyAndValue(item.Key, item.Value);
            }
            await SerializeDictionaryAsync();
            Interlocked.Exchange(ref _updatingConcurrently, 0);
        }
    }

    /// <summary>
    /// Upserts a value in the persistent dictionary based on the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key to upsert the value for.</param>
    /// <param name="value">The value to upsert.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public ValueTask UpsertAsync<T>(string key, T value) where T : struct, IConvertible {
        if (string.IsNullOrWhiteSpace(key)) {
            throw new ArgumentNullException(nameof(key));
        }

        return UpsertAsync(key, value.ToString() ?? "");
    }

    /// <summary>
    /// Deserializes the dictionary from its persisted state.
    /// </summary>
    protected abstract Dictionary<string, string>? Deserialize();

    /// <summary>
    /// Removes all keys and values from the dictionary.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public virtual async ValueTask ClearAsync() {
        if (Count is 0) {
            return;
        }
        _dict.Clear();
        await SerializeDictionaryAsync();
    }

    /// <summary>
    /// Serializes the contents of the dictionary to a persistent store.
    /// </summary>
    protected abstract Task SerializeAsync();

    /// <summary>
    /// Serializes the dictionary to a persistent store, while ensuring thread safety.
    /// </summary>
    /// <remarks>It is executed automatically after <see cref="UpsertAsync(string, string)"/>.</remarks>
    public virtual async Task SerializeDictionaryAsync() => await SerializeAsync();
}