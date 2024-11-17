using System.Buffers;

using Sharpify.Collections;

namespace Sharpify.Data;

internal readonly ref struct DisposableKey : IDisposable {
	private readonly IMemoryOwner<char> _owner;

	public readonly ReadOnlySpan<char> Key;

	public static DisposableKey Create(ReadOnlySpan<char> prefix, ReadOnlySpan<char> value) {
		return new DisposableKey(prefix, value);
	}

	private DisposableKey(ReadOnlySpan<char> prefix, ReadOnlySpan<char> value) {
		_owner = MemoryPool<char>.Shared.Rent(prefix.Length + value.Length);
		var buffer = StringBuffer.Create(_owner.Memory.Span);
		buffer.Append(prefix);
		buffer.Append(value);
		Key = buffer.WrittenSpan;
	}

	public DisposableKey() => throw new NotSupportedException("Use factory method");

    public void Dispose() {
        _owner.Dispose();
    }
}