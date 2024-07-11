namespace Sharpify;

/// <summary>
/// An iterator that can modify the value of a type
/// </summary>
/// <remarks>
/// Can be used with <see cref="ThreadSafe{T}"/> to avoid allocating a delegate for the modification.
/// </remarks>
public interface IModifier<T> {
    /// <summary>
    /// Modifies the value
    /// </summary>
    /// <returns>The modified value</returns>
    T Modify(T value, T newValue);
}