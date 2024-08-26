using System.Runtime.CompilerServices;

namespace Sharpify;

/// <summary>
/// Represents a set of concurrent utilities.
/// </summary>
public static class Concurrent {
	/// <summary>
	/// Processes all the values in precise batches according to the concurrency level.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="values"></param>
	/// <param name="consume"></param>
	/// <param name="concurrencyLevel"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public static ConfiguredTaskAwaitable ProcessAsync<T>(ReadOnlySpan<T> values,
									Func<T, CancellationToken, Task> consume,
									int concurrencyLevel,
									CancellationToken cancellationToken = default) {
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(concurrencyLevel, nameof(concurrencyLevel));

		if (concurrencyLevel > 1) {
			ReaderBoundChannel<T> channel = new(concurrencyLevel, cancellationToken);
			return channel.ProcessAsync(values, consume);
		} else { // concurrencyLevel == 1
			return ProcessAsyncSingleBatch(new UnsafeSpanAccessor<T>(values), consume, cancellationToken).ConfigureAwait(false);
		}

		static async Task ProcessAsyncSingleBatch(UnsafeSpanAccessor<T> values, Func<T, CancellationToken, Task> consume, CancellationToken cancellationToken) {
			int index = 0;
			while (index < values.Length) {
				await consume(values[index], cancellationToken);
				index++;
			}
		}
	}
}