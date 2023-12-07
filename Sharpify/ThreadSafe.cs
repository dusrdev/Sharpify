namespace Sharpify;

/// <summary>
/// A wrapper around a value that makes it thread safe.
/// </summary>
public sealed class ThreadSafe<T> {
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
    /// Provides a thread-safe way to modify the value.
    /// </summary>
    /// <returns>The value after the modification</returns>
    public T Modify(IModifier<T> modifier, T newValue) {
        lock (_lock) {
            _value = modifier.Modify(_value, newValue);
            return _value;
        }
    }
}