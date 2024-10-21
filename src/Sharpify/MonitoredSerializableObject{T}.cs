using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Sharpify.Collections;

namespace Sharpify;

/// <summary>
/// Represents a <see cref="SerializableObject{T}"/> that is monitored for changes from the file system.
/// </summary>
/// <typeparam name="T">The type of the value stored in the object.</typeparam>
/// <remarks>
/// This class provides functionality to serialize and deserialize the object to/from a file,
/// and raises an event whenever the file or the object is modified.
/// </remarks>
public class MonitoredSerializableObject<T> : SerializableObject<T> {
    private readonly FileSystemWatcher _watcher;

    /// <summary>
    /// Represents a serializable object that is monitored for changes in a specified file path.
    /// </summary>
    /// <param name="path">The path to the file. validated on creation</param>
    /// <param name="jsonTypeInfo">The json type info that can be used to serialize T without reflection</param>
    /// <exception cref="IOException">Thrown when the directory of the path does not exist or when the filename is invalid.</exception>
    public MonitoredSerializableObject(string path, JsonTypeInfo<T> jsonTypeInfo) : this(path, default!, jsonTypeInfo) { }

    /// <summary>
    /// Represents a serializable object that is monitored for changes in a specified file path.
    /// </summary>
    /// <param name="path">The path to the file. validated on creation</param>
    /// <param name="defaultValue">the default value of T, will be used if the file doesn't exist or can't be deserialized</param>
    /// <param name="jsonTypeInfo">The json type info that can be used to serialize T without reflection</param>
    /// <exception cref="IOException">Thrown when the directory of the path does not exist or when the filename is invalid.</exception>
    public MonitoredSerializableObject(string path, T defaultValue, JsonTypeInfo<T> jsonTypeInfo) : base(path, defaultValue, jsonTypeInfo) {
        _watcher = new FileSystemWatcher(_segmentedPath.Directory, _segmentedPath.FileName) {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };

        _watcher.Changed += OnFileChanged;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e) {
        if (e.ChangeType is not WatcherChangeTypes.Changed) {
            return;
        }
        if (!File.Exists(_path)) {
            return;
        }
        try {
            _lock.EnterWriteLock();
            using var file = File.OpenRead(_path);
            using var buffer = new RentedBufferWriter<byte>(_bufferSize);
            int written = file.Read(buffer.GetSpan());
            buffer.Advance(written);
            var reader = new Utf8JsonReader(buffer.WrittenSpan);
            _value = JsonSerializer.Deserialize(ref reader, _jsonTypeInfo)!;
            InvokeOnChangedEvent(_value);
        } finally {
            _lock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public override void Modify(Func<T, T> modifier) {
        _watcher.EnableRaisingEvents = false;
        base.Modify(modifier);
        _watcher.EnableRaisingEvents = true;
    }

    /// <inheritdoc/>
    public override void Dispose() {
        if (_disposed) {
            return;
        }
        _watcher?.Dispose();
        _lock?.Dispose();
        _disposed = true;
    }
}