using System.Runtime.CompilerServices;
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
internal readonly struct ReaderBoundChannel<T> {
	private readonly Channel<T> _channel;
	private readonly int _concurrencyLevel;
	private readonly CancellationToken _cancellationToken;

	/// <summary>
	/// Initializes a new instance of the <see cref="ReaderBoundChannel{T}"/> class.
	/// </summary>
	/// <param name="concurrencyLevel"></param>
	/// <param name="cancellationToken"></param>
	public ReaderBoundChannel(int concurrencyLevel, CancellationToken cancellationToken = default) {
		_concurrencyLevel = concurrencyLevel;
		_cancellationToken = cancellationToken;
		_channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions {
			AllowSynchronousContinuations = false,
			SingleWriter = false,
			SingleReader = false,
		});
	}

	/// <summary>
	/// Processes all the values until completion.
	/// </summary>
	/// <param name="values"></param>
	/// <param name="consume"></param>
	/// <returns></returns>
	public ConfiguredTaskAwaitable ProcessAsync(ReadOnlySpan<T> values, Func<T, CancellationToken, Task> consume) {
		var valuesEnumerator = new UnsafeSpanAccessor<T>(values);
		return Task.WhenAll(WriteAllAsync(valuesEnumerator), ConsumeAllAsync(consume)).ConfigureAwait(false);
	}

	private async Task WriteAllAsync(UnsafeSpanAccessor<T> values) {
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

	private async Task WriteChunkAsync(UnsafeSpanAccessor<T> valuesSlice) {
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
		using var buffer = new RentedBufferWriter<Task>(_concurrencyLevel);

		for (var i = _concurrencyLevel; i > 0; i--) {
			buffer.WriteAndAdvance(ConsumeAsync(consume));
		}

		await Task.WhenAll(buffer.WrittenSegment).WaitAsync(_cancellationToken).ConfigureAwait(false);
	}

	private async Task ConsumeAsync(Func<T, CancellationToken, Task> consume) {
		var reader = _channel.Reader;
		while (await reader.WaitToReadAsync(_cancellationToken)) {
			while (reader.TryRead(out var query)) {
				await consume(query, _cancellationToken).ConfigureAwait(false);
			}
		}
	}
}