using System.Text.Json;

namespace Sharpify.Collections;

/// <summary>
/// Represents a dictionary that persists its data to a local file but not in memory.
/// </summary>
public class LazyLocalPersistentDictionary : PersistentDictionary {
    private readonly string _path;
    private readonly StringComparer _stringComparer;
    private static readonly Dictionary<string, string> Empty = [];

    /// <summary>
    /// Creates a new instance of <see cref="LocalPersistentDictionary"/> with the <paramref name="path"/> and <paramref name="comparer"/> specified.
    /// </summary>
    /// <param name="path">The full path to the file to persist the dictionary to.</param>
    /// <param name="comparer">The comparer to use for the dictionary.</param>
    public LazyLocalPersistentDictionary(string path, StringComparer comparer) {
        _path = path;
        _stringComparer = comparer;
    }

    /// <summary>
    /// Creates a new instance of <see cref="LocalPersistentDictionary"/> with the <paramref name="path"/> and <see cref="StringComparer.Ordinal"/>.
    /// </summary>
    /// <param name="path">The path to the file to persist the dictionary to.</param>
    public LazyLocalPersistentDictionary(string path) : this(path, StringComparer.Ordinal) { }

    /// <summary>
    /// Retrieves the value associated with the specified key from the persistent dictionary.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>
    /// The value associated with the specified key if it exists in the dictionary; otherwise, null.
    /// </returns>
    protected override string? GetValueByKey(string key) {
        if (!File.Exists(_path)) {
            return null;
        }
        ReadOnlySpan<byte> jsonUtf8Bytes = File.ReadAllBytes(_path);
        var options = new JsonReaderOptions {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        };
        var reader = new Utf8JsonReader(jsonUtf8Bytes, options);
        while (reader.Read()) {
            if (reader.TokenType is not JsonTokenType.PropertyName) {
                continue;
            }
            var property = reader.GetString();
            if (!_stringComparer.Equals(property, key)) {
                _ = reader.TrySkip();
                continue;
            }
            reader.Read();
            var value = reader.GetString();
            return value;
        }
        return null;
    }

    /// <summary>
    /// Sets the key and value in the dictionary.
    /// If the dictionary file does not exist, a new dictionary is created and the key-value pair is added.
    /// If the dictionary file exists, the dictionary is deserialized and the key-value pair is added or updated.
    /// </summary>
    /// <param name="key">The key to set.</param>
    /// <param name="value">The value to set.</param>
    protected override void SetKeyAndValue(string key, string value) {
        if (!File.Exists(_path)) {
            _dict ??= new Dictionary<string, string>(_stringComparer);
            _dict[key] = value;
            return;
        }
        var sDict = Deserialize();
        if (sDict is null) {
            _dict ??= new Dictionary<string, string>(_stringComparer);
            _dict[key] = value;
            return;
        }
        _dict = sDict;
        _dict[key] = value;
    }

    /// <inheritdoc/>
    protected override Dictionary<string, string>? Deserialize() {
        var json = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json, InternalHelper.JsonOptions);
    }

    /// <inheritdoc/>
    protected override async Task SerializeAsync() {
        var json = JsonSerializer.Serialize(_dict, InternalHelper.JsonOptions);
        await File.WriteAllTextAsync(_path, json);
        _dict = Empty;
    }
}