using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sharpify;

/// <summary>
/// Represents a generic serializable object
/// </summary>
/// <typeparam name="T">The type of the value stored in the object.</typeparam>
/// <remarks>
/// This class provides functionality to serialize and deserialize the object to/from a file,
/// and raises an event whenever the object is modified.
/// </remarks>
public class SerializableObject<T> {
    /// <summary>
    /// Gets value of type T.
    /// </summary>
    public T Value { get; protected set; } = default!;

    /// <summary>
    /// The path of the serialized object.
    /// </summary>
    protected readonly string _path;

    /// <summary>
    /// The segmented path of the serialized object.
    /// </summary>
    protected readonly SegmentedPath _segmentedPath;

    /// <summary>
    /// The lock object used for thread synchronization.
    /// </summary>
    protected readonly object _lock = new();

    /// <summary>
    /// The JSON serializer options used for serializing and deserializing objects.
    /// </summary>
    protected static readonly JsonSerializerOptions Options = new() {
        WriteIndented = true,
        IncludeFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    /// <summary>
    /// Represents a serializable object that is monitored for changes in a specified file path.
    /// </summary>
    /// <param name="path">The path to the file. validated on creation</param>
    /// <exception cref="IOException">Thrown when the directory of the path does not exist or when the filename is invalid.</exception>
    public SerializableObject(string path) : this(path, default!) { }

    /// <summary>
    /// Represents a serializable object that is monitored for changes in a specified file path.
    /// </summary>
    /// <param name="path">The path to the file. validated on creation</param>
    /// <param name="defaultValue">the default value of T, will be used if the file doesn't exist or can't be deserialized</param>
    /// <exception cref="IOException">Thrown when the directory of the path does not exist or when the filename is invalid.</exception>
    public SerializableObject(string path, T defaultValue) {
        var dir = Path.GetDirectoryName(path);
        var fileName = Path.GetFileName(path);
        if (string.IsNullOrWhiteSpace(dir)) {
            throw new IOException("The directory of path does not exist");
        }
        if (string.IsNullOrWhiteSpace(fileName)) {
            throw new IOException("Filename is invalid");
        }
        _segmentedPath = new(dir, fileName);
        _path = path;
        if (File.Exists(path)) {
            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) {
                SetValueAndSerialize(defaultValue);
            } else {
                try {
                    Value = JsonSerializer.Deserialize<T>(json, Options)
                             ?? defaultValue;
                } catch {
                    SetValueAndSerialize(defaultValue);
                }
            }
        } else {
            SetValueAndSerialize(defaultValue);
        }
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
    /// <param name="e">An <see cref="SerializableObjectEventArgs{T}"/> that contains the event data.</param>
    public delegate void OnChangedEventHandler(object sender, SerializableObjectEventArgs<T> e);

    /// <summary>
    /// Event that is raised when the <see cref="Value"/> has changed.
    /// </summary>
    public event OnChangedEventHandler? OnChanged;

    /// <summary>
    /// Invokes the OnChanged event with the specified value.
    /// </summary>
    /// <param name="value">The value to pass to the event handler.</param>
    protected void InvokeOnChangedEvent(T value) {
        OnChanged?.Invoke(this, new SerializableObjectEventArgs<T>(value));
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e) {
        var json = File.ReadAllText(_path);
        if (string.IsNullOrWhiteSpace(json)) {
            return;
        }
        try {
            var serialized = JsonSerializer.Deserialize<T>(json, Options);
            Value = serialized!;
            InvokeOnChangedEvent(Value);
        } catch {
            return;
        }
    }

    /// <summary>
    /// Modifies the value of the object and performs necessary operations such as serialization and event invocation.
    /// </summary>
    /// <param name="modifier">The action that modifies the value of the object.</param>
    /// <remarks>A lock is used to prevent concurrent modifications</remarks>
    public virtual void Modify(Func<T, T> modifier) {
        lock (_lock) {
            Value = modifier(Value);
            File.WriteAllText(_path, JsonSerializer.Serialize(Value, Options));
            InvokeOnChangedEvent(Value);
        }
    }

    /// <summary>
    /// Represents a segmented path consisting of a directory and a file name.
    /// </summary>
    protected readonly record struct SegmentedPath(string Directory, string FileName);
}