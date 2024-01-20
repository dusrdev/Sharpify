using System.Buffers;

namespace Sharpify.Collections;

/// <summary>
/// A buffer writer that uses an array rented from the shared array pool
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// Essentially an allocation free alternative to <see cref="ArrayBufferWriter{T}"/>
/// </remarks>
public sealed class RentedBufferWriter<T> : IBufferWriter<T>, IDisposable {
	private readonly T[] _buffer;
	private int _index;

	/// <summary>
	/// The actual capacity of the rented buffer
	/// </summary>
	public readonly int ActualCapacity;

	/// <summary>
	/// Creates a new rented buffer writer with the at least the given capacity
	/// </summary>
	/// <param name="capacity">The actual buffer will be at least this size</param>
	public RentedBufferWriter(int capacity) {
#if NET8_0_OR_GREATER
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
#elif NET7_0
		if (capacity <= 0) {
			throw new ArgumentOutOfRangeException(nameof(capacity));
		}
#endif
		_buffer = ArrayPool<T>.Shared.Rent(capacity);
		ActualCapacity = _buffer.Length;
	}

	/// <inheritdoc />
	public void Advance(int count) {
#if NET8_0_OR_GREATER
		ArgumentOutOfRangeException.ThrowIfNegative(count);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(_index, _buffer.Length - count);
#elif NET7_0
		if (count < 0) {
			throw new ArgumentException(null, nameof(count));
		}
		if (_index > _buffer.Length - count) {
			throw new InvalidOperationException("Cannot advance past the end of the buffer.");
		}
#endif

		_index += count;
	}

	/// <summary>
	/// Returns the underlying buffer
	/// </summary>
	public T[] Buffer => _buffer;

	/// <summary>
	/// Gets the portion of the free buffer that can be written to, beginning at <see cref="_index"/>
	/// </summary>
	/// <param name="sizeHint">Not regarded</param>
	/// <returns></returns>
	public Memory<T> GetMemory(int sizeHint = 0) => _buffer.AsMemory(_index);

	/// <summary>
	/// Gets the portion of the free buffer that can be written to, beginning at <see cref="_index"/>
	/// </summary>
	/// <param name="sizeHint">Not regarded</param>
	/// <returns></returns>
	public Span<T> GetSpan(int sizeHint = 0) => _buffer.AsSpan(_index);

	/// <summary>
	/// Gets the portion of the buffer that has been written to, beginning at index 0
	/// </summary>
	public ArraySegment<T> WrittenSegment => new(_buffer, 0, _index);

	/// <summary>
	/// Gets the portion of the buffer that has been written to, beginning at index 0
	/// </summary>
	public ReadOnlyMemory<T> WrittenMemory => _buffer.AsMemory(0, _index);

	/// <summary>
	/// Gets the portion of the buffer that has been written to, beginning at index 0
	/// </summary>
	public ReadOnlySpan<T> WrittenSpan => _buffer.AsSpan(0, _index);

	/// <summary>
	/// Returns the number of elements that can be written to the buffer
	/// </summary>
	public int FreeCapacity => _buffer.Length - _index;

	/// <summary>
	/// Resets the buffer writer to its initial state by setting <see cref="_index"/> to 0
	/// </summary>
	public void Reset() => _index = 0;

	/// <summary>
	/// Returns a slice of the buffer
	/// </summary>
	/// <param name="start"></param>
	/// <param name="length"></param>
	/// <returns></returns>
	public ReadOnlyMemory<T> GetMemorySlice(int start, int length) {
#if NET8_0_OR_GREATER
		ArgumentOutOfRangeException.ThrowIfGreaterThan(start + length, _buffer.Length);
#elif NET7_0
		if (start + length > _buffer.Length) {
			throw new ArgumentOutOfRangeException();
		}
#endif
		return _buffer.AsMemory(start, length);
	}

	/// <summary>
	/// Returns a slice of the buffer
	/// </summary>
	/// <param name="start"></param>
	/// <param name="length"></param>
	/// <returns></returns>
	public ReadOnlySpan<T> GetSpanSlice(int start, int length) {
#if NET8_0_OR_GREATER
		ArgumentOutOfRangeException.ThrowIfGreaterThan(start + length, _buffer.Length);
#elif NET7_0
		if (start + length > _buffer.Length) {
			throw new ArgumentOutOfRangeException();
		}
#endif
		return _buffer.AsSpan(start, length);
	}

	/// <summary>
	/// Returns the rented buffer to the shared array pool
	/// </summary>
	public void Dispose() => ArrayPool<T>.Shared.Return(_buffer);
}