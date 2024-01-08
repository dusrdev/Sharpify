using System.Text.Json;

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
    private volatile uint _isInternalModification;

    /// <summary>
    /// Represents a serializable object that is monitored for changes in a specified file path.
    /// </summary>
    /// <param name="path">The path to the file. validated on creation</param>
    /// <exception cref="IOException">Thrown when the directory of the path does not exist or when the filename is invalid.</exception>
    public MonitoredSerializableObject(string path) : this(path, default!) { }

    /// <summary>
    /// Represents a serializable object that is monitored for changes in a specified file path.
    /// </summary>
    /// <param name="path">The path to the file. validated on creation</param>
    /// <param name="defaultValue">the default value of T, will be used if the file doesn't exist or can't be deserialized</param>
    /// <exception cref="IOException">Thrown when the directory of the path does not exist or when the filename is invalid.</exception>
    public MonitoredSerializableObject(string path, T defaultValue) : base(path, defaultValue) {
        _watcher = new FileSystemWatcher(_segmentedPath.Directory, _segmentedPath.FileName) {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };

        _watcher.Changed += OnFileChanged;
    }

    /// <summary>
    /// Represents a monitored serializable object.
    /// </summary>
    ~MonitoredSerializableObject() {
        Dispose();
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e) {
        if (Interlocked.Exchange(ref _isInternalModification, 0) is 1) {
            return;
        }
        _lock.EnterWriteLock();
        try {
            var json = File.ReadAllText(_path);
            if (string.IsNullOrWhiteSpace(json)) {
                return;
            }
            var serialized = JsonSerializer.Deserialize<T>(json, Options);
            if (serialized is null) {
                return;
            }
            _value = serialized;
            InvokeOnChangedEvent(_value);
        } finally {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Modifies the value of the object and performs necessary operations such as serialization and event invocation.
    /// </summary>
    /// <param name="modifier">The action that modifies the value of the object.</param>
    /// <remarks>A lock is used to prevent concurrent modifications</remarks>
    public override void Modify(Func<T, T> modifier) {
        _lock.EnterWriteLock();
        try {
            _value = modifier(_value);
            Interlocked.Exchange(ref _isInternalModification, 1);
            File.WriteAllText(_path, JsonSerializer.Serialize(_value, Options));
            InvokeOnChangedEvent(_value);
        } finally {
            _lock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public override void Dispose() {
        _watcher.Changed -= OnFileChanged;
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        _lock.Dispose();
        GC.SuppressFinalize(this);
    }
}