using System.Collections.Concurrent;

namespace Sharpify.Collections;

/// <summary>
/// Represents a custom persistent dictionary that extends the <see cref="PersistentDictionary"/> class.
/// </summary>
public sealed class CustomPersistentDictionary : PersistentDictionary {
    private readonly Func<ConcurrentDictionary<string, string>> _dictDeserializer;
    private readonly Func<Task> _dictSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomPersistentDictionary"/> class with the specified <paramref name="dictDeserializer"/>, <paramref name="dictSerializer"/> and <paramref name="comparer"/>.
    /// </summary>
    /// <param name="dictDeserializer">A function that deserializes the dictionary.</param>
    /// <param name="dictSerializer">A function that serializes the dictionary.</param>
    /// <param name="comparer">The string comparer to use for comparing keys.</param>
    public CustomPersistentDictionary(Func<ConcurrentDictionary<string, string>> dictDeserializer, Func<Task> dictSerializer, StringComparer comparer) {
        _dictDeserializer = dictDeserializer;
        _dictSerializer = dictSerializer;
        var sDict = DeserializeDictionary();
        if (sDict is null) {
            _dict = new ConcurrentDictionary<string, string>(comparer);
            return;
        }
        _dict = new ConcurrentDictionary<string, string>(sDict, comparer);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomPersistentDictionary"/> class with the specified <paramref name="dictDeserializer"/>, <paramref name="dictSerializer"/> and <see cref="StringComparer.Ordinal"/>.
    /// </summary>
    /// <param name="dictDeserializer">A function that deserializes the dictionary.</param>
    /// <param name="dictSerializer">A function that serializes the dictionary.</param>
    public CustomPersistentDictionary(Func<ConcurrentDictionary<string, string>> dictDeserializer, Func<Task> dictSerializer) : this (dictDeserializer, dictSerializer, StringComparer.Ordinal) {
    }

    /// <inheritdoc/>
    protected override ConcurrentDictionary<string, string>? DeserializeDictionary() => _dictDeserializer();

    /// <inheritdoc/>
    protected override async Task SerializeAsync() => await _dictSerializer();
}