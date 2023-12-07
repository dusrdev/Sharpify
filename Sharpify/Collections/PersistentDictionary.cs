using System.Collections.Concurrent;

namespace Sharpify.Collections;

/// <summary>
/// Provides a thread-safe concurrent dictionary that can be efficiently persisted.
/// </summary>
public abstract class PersistentDictionary {
    /// <summary>
    /// A thread-safe dictionary that stores string keys and values.
    /// </summary>
    protected ConcurrentDictionary<string, string>? _dict;

    private volatile int _pendingUpdates;

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <returns>The value associated with the specified key, or null if the key is not found.</returns>
    protected virtual string? GetValueByKey(string key) => _dict!.TryGetValue(key, out var value) ? value : null;

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    public string? this[string key] => GetValueByKey(key);

    /// <summary>
    /// Gets the value associated with the specified key, or creates a new key-value pair if the key does not exist.
    /// </summary>
    /// <param name="key">The key of the element to get or create.</param>
    /// <param name="default">The default value to use if the key does not exist.</param>
    /// <returns>The value associated with the specified key, or the default value if the key does not exist.</returns>
    public async Task<string> GetOrCreateAsync(string key, string @default) {
        if (_dict!.TryGetValue(key, out var value)) {
            return value;
        }

        await Upsert(key, @default);
        return @default;
    }

    /// <summary>
    /// Sets the specified key and value in the dictionary.
    /// </summary>
    /// <param name="key">The key to set.</param>
    /// <param name="value">The value to set.</param>
    protected virtual void SetKeyAndValue(string key, string value) => _dict![key] = value;

    /// <summary>
    /// Inserts or updates a key-value pair in the JSON file.
    /// </summary>
    /// <param name="key">The key to insert or update.</param>
    /// <param name="value">The value to insert or update.</param>
    public async Task Upsert(string key, string value) {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) {
            return;
        }

        // This allows to serialize only once when all pending updates are done
        // Reducing the number of expensive serialize operations

        Interlocked.Increment(ref _pendingUpdates);
        SetKeyAndValue(key, value);

        if (Interlocked.Decrement(ref _pendingUpdates) is 0) {
            await SerializeDictionaryAsync();
        }
    }

    /// <summary>
    /// Deserializes the dictionary from its persisted state.
    /// </summary>
    protected abstract ConcurrentDictionary<string, string>? Deserialize();

    /// <summary>
    /// Removes all keys and values from the dictionary.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public async ValueTask ClearAsync() {
        if (_dict is null || _dict.Count is 0) {
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
    /// <remarks>It is executed automatically after <see cref="Upsert(string, string)"/>.</remarks>
    public async Task SerializeDictionaryAsync() {
        await _semaphore.WaitAsync();
        try {
            await SerializeAsync();
        } finally {
            _semaphore.Release();
        }
    }
}