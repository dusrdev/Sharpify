namespace Sharpify;

/// <summary>
/// A wrapper around a value that makes it thread safe.
/// </summary>
public sealed class ThreadSafe<T> : IEquatable<T>, IEquatable<ThreadSafe<T>> {
    //TODO: Switch to NET9 new Lock type
    private readonly object _lock = new();
    private T _value;

    /// <summary>
    /// Creates a new instance of ThreadSafe with an initial value.
    /// </summary>
    public ThreadSafe(T value) {
        _value = value;
    }

    /// <summary>
    /// Creates a new instance of ThreadSafe with the default value of T.
    /// </summary>
    public ThreadSafe() : this(default!) { }

    /// <summary>
    /// A public getter and setter for the value.
    /// </summary>
    /// <remarks>
    /// The inner operation are thread-safe, use this to change or access the value.
    /// </remarks>
    public T Value {
        get {
            lock (_lock) {
                return _value;
            }
        }
    }

    /// <summary>
    /// Provides a thread-safe way to modify the value.
    /// </summary>
    /// <returns>The value after the modification</returns>
    public T Modify(Func<T, T> modificationFunc) {
        lock (_lock) {
            _value = modificationFunc(_value);
            return _value;
        }
    }

    /// <summary>
    /// Checks if the value is equal to the other value.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(ThreadSafe<T>? other) => other is not null && Equals(other.Value);

    /// <summary>
    /// Checks if the value is equal to the other value.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(T? other) {
        if (other is null) {
            return false;
        }
        if (_value is null) {
            return false;
        }
        return _value.Equals(other);
    }

    /// <summary>
    /// Checks if the value is equal to the other value.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj) {
        return Equals(obj as ThreadSafe<T>);
    }

    /// <summary>
    /// Gets the hash code of the value.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => Value!.GetHashCode();

}