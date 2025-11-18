using System.Buffers;

namespace Sharpify;

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

	internal PooledArrayOwner(ArrayPool<T> pool, int minimumLength) {
		_pool = pool;
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

/// <summary>
/// Provides any <see cref="ArrayPool{T}"/> extensions that return a <see cref="PooledArrayOwner{T}"/>.
/// </summary>
public static class ArrayPoolExtensions {
	/// <summary>
	/// Rent an array with <paramref name="minimumLength"/> from an <see cref="ArrayPool{T}"/> , returning a struct that will return it after being disposed, and also the held array reference.
	/// </summary>
	/// <param name="pool"></param>
	/// <param name="minimumLength"></param>
	/// <param name="array"></param>
	/// <returns></returns>
	public static PooledArrayOwner<T> Rent<T>(this ArrayPool<T> pool, int minimumLength, out T[] array) {
		PooledArrayOwner<T> owner = new(pool, minimumLength);
		array = owner.Value;
		return owner;
	}

	/// <summary>
	/// Rent an array with <paramref name="minimumLength"/> from an <see cref="ArrayPool{T}"/>, returning a struct that will return it after being disposed, and also a <see cref="Span{T}"/> over the held array reference.
	/// </summary>
	/// <param name="pool"></param>
	/// <param name="minimumLength"></param>
	/// <param name="span"></param>
	/// <returns></returns>
	public static PooledArrayOwner<T> Rent<T>(this ArrayPool<T> pool, int minimumLength, out Span<T> span) {
		PooledArrayOwner<T> owner = new(pool, minimumLength);
		span = owner.Value.AsSpan();
		return owner;
	}

	/// <summary>
	/// Rent an array with <paramref name="minimumLength"/> from an <see cref="ArrayPool{T}"/>, returning a struct that will return it after being disposed, and also a <see cref="Memory{T}"/> over the held array reference.
	/// </summary>
	/// <param name="pool"></param>
	/// <param name="minimumLength"></param>
	/// <param name="memory"></param>
	/// <returns></returns>
	public static PooledArrayOwner<T> Rent<T>(this ArrayPool<T> pool, int minimumLength, out Memory<T> memory) {
		PooledArrayOwner<T> owner = new(pool, minimumLength);
		memory = owner.Value.AsMemory();
		return owner;
	}
}