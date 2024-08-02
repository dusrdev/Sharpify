using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Sharpify;

/// <summary>
/// Represents a generic serializable object
/// </summary>
/// <typeparam name="T">The type of the value stored in the object.</typeparam>
/// <remarks>
/// This class provides functionality to serialize and deserialize the object to/from a file,
/// and raises an event whenever the object is modified.
/// </remarks>
public class SerializableObject<T> : IDisposable {
    /// <summary>
    /// The value of the SerializableObject.
    /// </summary>
    protected T _value = default!;

    /// <summary>
    /// Gets value of type T.
    /// </summary>
    public T Value {
        get {
            try {
                _lock.EnterReadLock();
                return _value;
            } finally {
                _lock.ExitReadLock();
            }
        }
    }

    private volatile bool _disposed;

    /// <summary>
    /// The path of the serialized object.
    /// </summary>
    protected readonly string _path;

    /// <summary>
    /// The segmented path of the serialized object.
    /// </summary>
    protected readonly SegmentedPath _segmentedPath;

    /// <summary>
    /// The JSON type info used for serializing and deserializing objects.
    /// </summary>
    protected readonly JsonTypeInfo<T> _jsonTypeInfo;

    /// <summary>
    /// The lock object used for thread synchronization.
    /// </summary>
    protected readonly ReaderWriterLockSlim _lock = new();

    /// <summary>
    /// Represents a serializable object that is monitored for changes in a specified file path.
    /// </summary>
    /// <param name="path">The path to the file. validated on creation</param>
    /// <param name="jsonTypeInfo">The json type info that can be used to serialize T without reflection</param>
    /// <exception cref="IOException">Thrown when the directory of the path does not exist or when the filename is invalid.</exception>
    public SerializableObject(string path, JsonTypeInfo<T> jsonTypeInfo) : this(path, default!, jsonTypeInfo) { }

    /// <summary>
    /// Represents a serializable object that is monitored for changes in a specified file path.
    /// </summary>
    /// <param name="path">The path to the file. validated on creation</param>
    /// <param name="defaultValue">the default value of T, will be used if the file doesn't exist or can't be deserialized</param>
    /// <param name="jsonTypeInfo">The json type info that can be used to serialize T without reflection</param>
    /// <exception cref="IOException">Thrown when the directory of the path does not exist or when the filename is invalid.</exception>
    public SerializableObject(string path, T defaultValue, JsonTypeInfo<T> jsonTypeInfo) {
        _jsonTypeInfo = jsonTypeInfo;
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
        if (Path.Exists(path)) {
            var length = checked((int)new FileInfo(path).Length);
            var textLength = length / sizeof(char);
            if (textLength is 0) {
                SetValueAndSerialize(defaultValue);
            } else {
                try {
                    using var file = File.Open(path, FileMode.Open);
                    _value = JsonSerializer.Deserialize(file, _jsonTypeInfo)!;
                } catch {
                    SetValueAndSerialize(defaultValue);
                }
            }
        } else {
            SetValueAndSerialize(defaultValue);
        }
    }

    private void SetValueAndSerialize(T value) {
        try {
            _lock.EnterWriteLock();
            _value = value;
            using var file = File.Open(_path, FileMode.Create);
            JsonSerializer.Serialize(file, _value, _jsonTypeInfo);
        } finally {
            _lock.ExitWriteLock();
        }
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

    /// <summary>
    /// Modifies the value of the object and performs necessary operations such as serialization and event invocation.
    /// </summary>
    /// <param name="modifier">The action that modifies the value of the object.</param>
    /// <remarks>
    /// <para>
    /// a lock is used to prevent concurrent modifications
    /// </para>
    /// <para>
    /// When a record (non-struct) is used, do not use the "with" keyword to return a modification, this will allocate a new object, instead modify the existing and return the object, this will circularly exchange the reference.
    /// </para>
    /// </remarks>
    public virtual void Modify(Func<T, T> modifier) {
        try {
            _lock.EnterWriteLock();
            _value = modifier(_value);
            using var file = File.Open(_path, FileMode.Create);
            JsonSerializer.Serialize(file, _value, _jsonTypeInfo);
            InvokeOnChangedEvent(_value);
        } finally {
            _lock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public virtual void Dispose() {
        if (_disposed) {
            return;
        }
        _lock?.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// Represents a segmented path consisting of a directory and a file name.
    /// </summary>
    protected readonly record struct SegmentedPath(string Directory, string FileName);
}