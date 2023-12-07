using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace Sharpify.Collections;

/// <summary>
/// Provides a thread-safe concurrent dictionary that can be efficiently persisted.
/// </summary>
public abstract class PersistentDictionary {
    /// <summary>
    /// A thread-safe dictionary that stores string keys and values.
    /// </summary>
    protected ConcurrentDictionary<string, string>? _dict;

    private readonly ConcurrentQueue<(string Key, string Value)> _queue = new();

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private volatile int _pendingUpdates;

    /// <summary>
    /// Gets the number of key-value pairs contained in the PersistentDictionary.
    /// </summary>
    public int Count => _dict is null ? 0 : _dict.Count;

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <returns>The value associated with the specified key, or null if the key is not found.</returns>
    protected virtual string? GetValueByKey(string key) => _dict!.TryGetValue(key, out var value) ? value : null;

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
    /// Inserts or updates a key-value pair in the dictionary and serializes.
    /// </summary>
    /// <param name="key">The key to insert or update.</param>
    /// <param name="value">The value to insert or update.</param>
    // public virtual async ValueTask Upsert(string key, string value) {
    //     if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) {
    //         return;
    //     }

    //     // Reduce the number of times we need to serialize the dictionary.

    //     Interlocked.Increment(ref _pendingUpdates);
    //     SetKeyAndValue(key, value);

    //     if (Interlocked.Decrement(ref _pendingUpdates) != 0) {
    //         return;
    //     }

    //     await _semaphore.WaitAsync();
    //     try {
    //         if (_pendingUpdates is 0) {
    //             await SerializeDictionaryAsync();
    //         }
    //     } finally {
    //         _semaphore.Release();
    //     }
    // }
    public virtual async ValueTask Upsert(string key, string value) {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) {
            return;
        }

        // Reduce the number of times we need to serialize the dictionary.


        await _semaphore.WaitAsync();
        SetKeyAndValue(key, value);
        await SerializeDictionaryAsync();
        _semaphore.Release();
    }

    /// <summary>
    /// Deserializes the dictionary from its persisted state.
    /// </summary>
    protected abstract ConcurrentDictionary<string, string>? Deserialize();

    /// <summary>
    /// Removes all keys and values from the dictionary.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public virtual async ValueTask ClearAsync() {
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
    public virtual async Task SerializeDictionaryAsync() => await SerializeAsync();
}