namespace Sharpify;

/// <summary>
/// Provides a wrapper around <typeparamref name="T"/> that allows thread-safe access and update, with an optional callback function whenever the value is updated.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class Synchronized<T> {
	private readonly Action<T>? _onUpdate;

	/// <summary>
	/// The value held by this reference of <see cref="Synchronized{T}"/>, get and set are both thread-safe.
	/// </summary>
	public T Value {
		get {
			return field;
		}
		set {
			Interlocked.Exchange(ref field, value);
			if (_onUpdate is not null) _onUpdate(field);
		}
	}

	/// <summary>
	/// Creates a new instance of <see cref="Synchronized{T}"/> with an <paramref name="initialValue"/> and optional <paramref name="onUpdate"/> callback.
	/// </summary>
	/// <param name="initialValue">The initial value to used.</param>
	/// <param name="onUpdate">The action that will be executed when the value is updated.</param>
	public Synchronized(T initialValue, Action<T>? onUpdate = null) {
		Value = initialValue;
		_onUpdate = onUpdate;
	}
}