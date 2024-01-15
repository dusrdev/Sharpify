using System.Text.Json;

namespace Sharpify.Collections;

/// <summary>
/// Represents a dictionary that persists its data to a local file.
/// </summary>
public class LocalPersistentDictionary : PersistentDictionary {
    private readonly string _path;

    /// <summary>
    /// Creates a new instance of <see cref="LocalPersistentDictionary"/> with the <paramref name="path"/> and <paramref name="comparer"/> specified.
    /// </summary>
    /// <param name="path">The full path to the file to persist the dictionary to.</param>
    /// <param name="comparer">The comparer to use for the dictionary.</param>
    public LocalPersistentDictionary(string path, StringComparer comparer) {
        _path = path;
        if (!File.Exists(_path)) {
            _dict = new Dictionary<string, string>(comparer);
            return;
        }
        var sDict = Deserialize();
        if (sDict is null) {
            _dict = new Dictionary<string, string>(comparer);
            return;
        }
        _dict = new Dictionary<string, string>(sDict, comparer);
    }

    /// <summary>
    /// Creates a new instance of <see cref="LocalPersistentDictionary"/> with the <paramref name="path"/> and <see cref="StringComparer.Ordinal"/>.
    /// </summary>
    /// <param name="path">The path to the file to persist the dictionary to.</param>
    public LocalPersistentDictionary(string path) : this(path, StringComparer.Ordinal) { }

    /// <inheritdoc/>
    protected override Dictionary<string, string>? Deserialize() {
        using var file = File.Open(_path, FileMode.Open);
        return JsonSerializer.Deserialize(file, JsonContext.Default.DictionaryStringString);
    }

    /// <inheritdoc/>
    protected override async Task SerializeAsync() {
        await using var file = File.Open(_path, FileMode.Create);
        await JsonSerializer.SerializeAsync(file, _dict, JsonContext.Default.DictionaryStringString);
    }
}