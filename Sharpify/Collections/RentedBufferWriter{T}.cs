using System.Buffers;

namespace Sharpify.Collections;

/// <summary>
/// A buffer writer that uses an array rented from the shared array pool
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class RentedBufferWriter<T> : IBufferWriter<T>, IDisposable {
	private readonly T[] buffer;
	private int _index;

	/// <summary>
	/// Creates a new rented buffer writer with the at least the given capacity
	/// </summary>
	public RentedBufferWriter(int capacity) {
		buffer = ArrayPool<T>.Shared.Rent(capacity);
	}

/// <inheritdoc />
	public void Advance(int count) {
		if (count < 0) {
			throw new ArgumentException(null, nameof(count));
		}

		if (_index > buffer.Length - count) {
			throw new InvalidOperationException("Cannot advance past the end of the buffer.");
		}

		_index += count;
	}

	/// <summary>
	/// Gets the portion of the free buffer that can be written to, beginning at <see cref="_index"/>
	/// </summary>
	/// <param name="sizeHint">Not regarded</param>
	/// <returns></returns>
	public Memory<T> GetMemory(int sizeHint = 0) {
		return buffer.AsMemory(_index);
	}

	/// <summary>
	/// Gets the portion of the free buffer that can be written to, beginning at <see cref="_index"/>
	/// </summary>
	/// <param name="sizeHint">Not regarded</param>
	/// <returns></returns>
	public Span<T> GetSpan(int sizeHint = 0) {
		return buffer.AsSpan(_index);
	}

	/// <summary>
	/// Gets the portion of the buffer that has been written to, beginning at index 0
	/// </summary>
	public Memory<T> WrittenMemory => buffer.AsMemory(0, _index);

	/// <summary>
	/// Gets the portion of the buffer that has been written to, beginning at index 0
	/// </summary>
	public Span<T> WrittenSpan => buffer.AsSpan(0, _index);

	/// <summary>
	/// Returns the rented buffer to the shared array pool
	/// </summary>
	public void Dispose() {
		ArrayPool<T>.Shared.Return(buffer);
	}
}