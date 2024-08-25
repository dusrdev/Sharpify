using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;

using Sharpify.Collections;

namespace Sharpify;

/// <summary>
/// Represents a single-writer, reader bound channel for precise batched parallelism.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// <para>
/// This abstraction is useful to concurrently handle precise batches of tasks running on a single collection of data.
/// </para>
/// <para>
/// This channel is not an alternative to <see cref="Task.WhenAll(IEnumerable{Task})"/>, for small and quick tasks, this would likely be slower and have more overhead. But for long running complex tasks where batch size is important, this abstraction can be very powerful.
/// </para>
/// </remarks>
public class ReaderBoundChannel<T> {
	private readonly Channel<T> _channel;
	private readonly int _concurrencyLevel;
	private readonly CancellationToken _cancellationToken;

	/// <summary>
	/// Initializes a new instance of the <see cref="ReaderBoundChannel{T}"/> class.
	/// </summary>
	/// <param name="cancellationToken"></param>
	public ReaderBoundChannel(CancellationToken cancellationToken = default) : this(
		1,
		cancellationToken) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="ReaderBoundChannel{T}"/> class.
	/// </summary>
	/// <param name="concurrencyLevel">The number of concurrent readers</param>
	/// <param name="cancellationToken"></param>
	public ReaderBoundChannel(int concurrencyLevel, CancellationToken cancellationToken = default) {
		_concurrencyLevel = concurrencyLevel;
		_cancellationToken = cancellationToken;
		_channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions {
			AllowSynchronousContinuations = false,
			SingleWriter = concurrencyLevel is 1,
			SingleReader = concurrencyLevel is 1,
		});
	}

    /// <summary>
    /// Processes all the values until completion.
    /// </summary>
    /// <param name="values"></param>
    /// <param name="consume"></param>
    /// <returns></returns>
    public ConfiguredTaskAwaitable ProcessAsync(ReadOnlySpan<T> values, Func<T, CancellationToken, Task> consume) {
        var valuesEnumerator = new ReadOnlySpanEnumerator(values);
		return Task.WhenAll(WriteAllAsync(valuesEnumerator), ConsumeAllAsync(consume)).ConfigureAwait(false);
	}

	private async Task WriteAllAsync(ReadOnlySpanEnumerator values) {
		if (_concurrencyLevel is 1) {
			await WriteChunkAsync(values.Slice(0, values.Length)).ConfigureAwait(false);
			_channel.Writer.TryComplete();
			return;
		} else {
			using var buffer = new RentedBufferWriter<Task>(_concurrencyLevel);
			int chunkSize = (int)Math.Ceiling((double)values.Length / _concurrencyLevel);

			for (var i = 0; i < values.Length; i += chunkSize) {
                if (i + chunkSize >= values.Length) {
                    buffer.WriteAndAdvance(WriteChunkAsync(values.Slice(i, values.Length - i)));
                    break;
                } else {
                    buffer.WriteAndAdvance(WriteChunkAsync(values.Slice(i, chunkSize)));
                }
			}

			await Task.WhenAll(buffer.WrittenSegment).WaitAsync(_cancellationToken).ConfigureAwait(false);
			_channel.Writer.TryComplete();
		}
	}

	private async Task WriteChunkAsync(ReadOnlySpanEnumerator valuesSlice) {
        int length = valuesSlice.Length;
        int index = 0;
		while (index < length && await _channel.Writer.WaitToWriteAsync(_cancellationToken)) {
            if (_channel.Writer.TryWrite(valuesSlice[index])) {
                index++;
            } else {
                break;
            }
        }
	}

	private async Task ConsumeAllAsync(Func<T, CancellationToken, Task> consume) {
		if (_concurrencyLevel is 1) {
			await ConsumeAsync(consume).ConfigureAwait(false);
			return;
		} else {
			using var buffer = new RentedBufferWriter<Task>(_concurrencyLevel);

			for (var i = _concurrencyLevel; i > 0; i--) {
				buffer.WriteAndAdvance(ConsumeAsync(consume));
			}

			await Task.WhenAll(buffer.WrittenSegment).WaitAsync(_cancellationToken).ConfigureAwait(false);
		}
	}

	private async Task ConsumeAsync(Func<T, CancellationToken, Task> consume) {
		var reader = _channel.Reader;
		while (await reader.WaitToReadAsync(_cancellationToken)) {
			while (reader.TryRead(out var query)) {
				await consume(query, _cancellationToken).ConfigureAwait(false);
			}
		}
	}

	/// <summary>
	/// A struct to enumerate a <see cref="ReadOnlySpan{T}" /> without capturing the reference
	/// </summary>
	private unsafe readonly struct ReadOnlySpanEnumerator {
		// Should not be required in .NET 9
		private readonly void* _pointer;
		public readonly int Length;

		public ReadOnlySpanEnumerator(ReadOnlySpan<T> span) {
			_pointer = Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
			Length = span.Length;
		}

        private ReadOnlySpanEnumerator(void* start, int length) {
            _pointer = start;
            Length = length;
        }

        public ReadOnlySpanEnumerator Slice(int start, int length) {
            return new ReadOnlySpanEnumerator(Unsafe.Add<T>(_pointer, start), length);
        }

		public ref readonly T this[int index] {
			get {
				void* item = Unsafe.Add<T>(_pointer, index);
				return ref Unsafe.AsRef<T>(item);
			}
		}

		public IEnumerable<T> ToEnumerable() {
			for (var i = 0; i < Length; i++) {
				yield return this[i];
			}
		}
	}
}