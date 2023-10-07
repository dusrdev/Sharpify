using System.Collections.Concurrent;

namespace Sharpify.Collections;

/// <summary>
/// Provides a thread-safe way to read and write JSON data to a file using a dictionary-like interface.
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
    public string? this[string key] => _dict!.TryGetValue(key, out var value) ? value : null;

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
        _dict![key] = value;

        if (Interlocked.Decrement(ref _pendingUpdates) is 0) {
            await SerializeDictionaryAsync();
        }
    }

    /// <summary>
    /// Deserializes the dictionary from its persisted state.
    /// </summary>
    protected abstract ConcurrentDictionary<string, string>? DeserializeDictionary();

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