using System.Collections.Concurrent;
using System.Text.Json;

namespace Sharpify.Collections;

/// <summary>
/// Represents a dictionary that persists its data to a local file.
/// </summary>
public sealed class LocalPersistentDictionary : PersistentDictionary {
    private readonly string _path;

    /// <summary>
    /// Creates a new instance of <see cref="LocalPersistentDictionary"/> with the <paramref name="path"/> and <paramref name="comparer"/> specified.
    /// </summary>
    /// <param name="path">The path to the file to persist the dictionary to.</param>
    /// <param name="comparer">The comparer to use for the dictionary.</param>
    public LocalPersistentDictionary(string path, StringComparer comparer) {
        _path = path;
        if (!File.Exists(_path)) {
            _dict = new ConcurrentDictionary<string, string>(comparer);
            return;
        }
        var sDict = DeserializeDictionary();
        if (sDict is null) {
            _dict = new ConcurrentDictionary<string, string>(comparer);
            return;
        }
        _dict = new ConcurrentDictionary<string, string>(sDict, comparer);
    }

    /// <summary>
    /// Creates a new instance of <see cref="LocalPersistentDictionary"/> with the <paramref name="path"/> and <see cref="StringComparer.Ordinal"/>.
    /// </summary>
    /// <param name="path">The path to the file to persist the dictionary to.</param>
    public LocalPersistentDictionary(string path) : this(path, StringComparer.Ordinal) { }

    private static readonly JsonSerializerOptions Options = new() {
        WriteIndented = true
    };

    /// <inheritdoc/>
    protected override ConcurrentDictionary<string, string>? DeserializeDictionary() {
        var json = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<ConcurrentDictionary<string, string>>(json, Options);
    }

    /// <inheritdoc/>
    protected override async Task SerializeAsync() {
        var json = JsonSerializer.Serialize(_dict, Options);
        await File.WriteAllTextAsync(_path, json);
    }
}