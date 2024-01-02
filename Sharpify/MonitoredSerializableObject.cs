using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sharpify;

/// <summary>
/// Represents a generic serializable object that is monitored for changes in a file system.
/// </summary>
/// <typeparam name="T">The type of the value stored in the object.</typeparam>
/// <remarks>
/// This class provides functionality to serialize and deserialize the object to/from a file,
/// and raises an event whenever the file or the object is modified.
/// </remarks>
public class MonitoredSerializableObject<T> : IDisposable {
    /// <summary>
    /// Gets value of type T.
    /// </summary>
    public T Value { get; private set; } = default!;

    private readonly string _path;
    private readonly FileSystemWatcher _watcher;
    private volatile uint _isInternalModification = 0;
    private readonly object _lock = new();

    private static readonly JsonSerializerOptions Options = new() {
        WriteIndented = true,
        IncludeFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

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
    public MonitoredSerializableObject(string path, T defaultValue) {
        var dir = Path.GetDirectoryName(path);
        var fileName = Path.GetFileName(path);
        if (string.IsNullOrWhiteSpace(dir)) {
            throw new IOException("The directory of path does not exist");
        }
        if (string.IsNullOrWhiteSpace(fileName)) {
            throw new IOException("Filename is invalid");
        }
        _path = path;
        if (File.Exists(path)) {
            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) {
                SetValueAndSerialize(defaultValue);
            } else {
                Value = JsonSerializer.Deserialize<T>(json, Options)
                         ?? defaultValue;
            }
        } else {
            SetValueAndSerialize(defaultValue);
        }

        _watcher = new FileSystemWatcher(dir, fileName) {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };

        _watcher.Changed += OnFileChanged;
    }

    private void SetValueAndSerialize(T value) {
        Value = value;
        var json = JsonSerializer.Serialize(Value, Options);
        File.WriteAllText(_path, json);
    }

    /// <summary>
    /// Represents the method that will handle the OnChanged" event.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="MonitorObjectEventArgs"/> that contains the event data.</param>
    public delegate void OnChangedEventHandler(object sender, MonitorObjectEventArgs e);

    /// <summary>
    /// Event that is raised when the <see cref="Value"/> has changed.
    /// </summary>
    public event OnChangedEventHandler? OnChanged;

    private void OnFileChanged(object sender, FileSystemEventArgs e) {
        if (Interlocked.Exchange(ref _isInternalModification, 0) is 1) {
            return;
        }
        var json = File.ReadAllText(_path);
        if (string.IsNullOrWhiteSpace(json)) {
            return;
        }
        var serialized = JsonSerializer.Deserialize<T>(json, Options);
        if (serialized is null) {
            return;
        }
        Value = serialized;
        OnChanged?.Invoke(this, new MonitorObjectEventArgs(Value));
    }

    /// <summary>
    /// Modifies the value of the object and performs necessary operations such as serialization and event invocation.
    /// </summary>
    /// <param name="modifier">The action that modifies the value of the object.</param>
    /// <remarks>A lock is used to prevent concurrent modifications</remarks>
    public void Modify(Func<T, T> modifier) {
        lock (_lock) {
            Value = modifier(Value);
            Interlocked.Exchange(ref _isInternalModification, 1);
            File.WriteAllText(_path, JsonSerializer.Serialize(Value, Options));
            OnChanged?.Invoke(this, new MonitorObjectEventArgs(Value));
        }
    }

    /// <inheritdoc/>
    public void Dispose() {
        _watcher.Changed -= OnFileChanged;
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        OnChanged = null;
    }


    /// <summary>
    /// Represents the event arguments for a monitor object.
    /// </summary>
    public class MonitorObjectEventArgs : EventArgs {
        /// <summary>
        /// Gets the value associated with the event.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonitorObjectEventArgs"/> class with the specified value.
        /// </summary>
        /// <param name="value">The value associated with the event.</param>
        public MonitorObjectEventArgs(T value) => Value = value;
    }
}