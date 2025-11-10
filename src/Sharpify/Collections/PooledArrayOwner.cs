using System.Buffers;

namespace Sharpify.Collections;

/// <summary>
/// A struct that allows renting an array from <see cref="ArrayPool{T}.Shared"/> and manage its return using the <see cref="IDisposable"/> interface.
/// </summary>
/// <typeparam name="T"></typeparam>
public partial struct PooledArrayOwner<T> : IDisposable {
	private readonly ArrayPool<T> _pool;
	private bool _disposed;

	/// <summary>
	/// The rented array held by this object.
	/// </summary>
	public readonly T[] Value;

	private PooledArrayOwner(int minimumLength) {
		_pool = ArrayPool<T>.Shared;
		Value = _pool.Rent(minimumLength);
	}

	/// <summary>
	/// Returns the rented array back to <see cref="ArrayPool{T}.Shared"/>.
	/// </summary>
	public void Dispose() {
		if (_disposed) return;
		_pool.Return(Value);
		_disposed = true;
	}
}

public partial struct PooledArrayOwner<T> : IDisposable {
	/// <summary>
	/// Rent an array with <paramref name="minimumLength"/> from <see cref="ArrayPool{T}.Shared"/>, returning a struct that will return it after being disposed, and also the held array reference.
	/// </summary>
	/// <param name="minimumLength"></param>
	/// <param name="array"></param>
	/// <returns></returns>
	public static PooledArrayOwner<T> Rent(int minimumLength, out T[] array) {
		PooledArrayOwner<T> owner = new(minimumLength);
		array = owner.Value;
		return owner;
	}

	/// <summary>
	/// Rent an array with <paramref name="minimumLength"/> from <see cref="ArrayPool{T}.Shared"/>, returning a struct that will return it after being disposed, and also a <see cref="Span{T}"/> over the held array reference.
	/// </summary>
	/// <param name="minimumLength"></param>
	/// <param name="span"></param>
	/// <returns></returns>
	public static PooledArrayOwner<T> Rent(int minimumLength, out Span<T> span) {
		PooledArrayOwner<T> owner = new(minimumLength);
		span = owner.Value.AsSpan();
		return owner;
	}

	/// <summary>
	/// Rent an array with <paramref name="minimumLength"/> from <see cref="ArrayPool{T}.Shared"/>, returning a struct that will return it after being disposed, and also a <see cref="Memory{T}"/> over the held array reference.
	/// </summary>
	/// <param name="minimumLength"></param>
	/// <param name="memory"></param>
	/// <returns></returns>
	public static PooledArrayOwner<T> Rent(int minimumLength, out Memory<T> memory) {
		PooledArrayOwner<T> owner = new(minimumLength);
		memory = owner.Value.AsMemory();
		return owner;
	}
}