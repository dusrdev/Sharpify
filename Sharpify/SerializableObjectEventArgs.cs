namespace Sharpify;

/// <summary>
/// Represents the event arguments for a serializable object.
/// </summary>
public class SerializableObjectEventArgs<T> : EventArgs {
    /// <summary>
    /// Gets the value associated with the event.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializableObjectEventArgs{T}"/> class with the specified value.
    /// </summary>
    /// <param name="value">The value associated with the event.</param>
    public SerializableObjectEventArgs(T value) => Value = value;
}