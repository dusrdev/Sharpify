using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Sharpify;

public static partial class Extensions {
    /// <summary>
    /// Returns a <see cref="Concurrent{T}"/> wrapper for the given collection.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Concurrent<T> Concurrent<T>(this ICollection<T> source) => source is null
        ? throw new ArgumentNullException(nameof(source))
        : new Concurrent<T>(source);

    /// <summary>
    /// An extension method to perform an action on a collection of items in parallel.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static Task InvokeAsync<T>(
        this Concurrent<T> concurrentReference,
        in IAsyncAction<T> action,
        CancellationToken token = default) {
        var length = concurrentReference.Source.Count;
        if (length is 0) {
            return Task.CompletedTask;
        }
        var tasks = new Task[length];

#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfNotEqual(tasks.Length, concurrentReference.Source.Count);
        // Jit should use the exception to optimize the bounds check away
#endif

        Span<Task> taskSpan = tasks;
        var i = 0;
        foreach (var item in concurrentReference.Source) {
            taskSpan[i++] = action.InvokeAsync(item);
        }
        return Task.WhenAll(tasks).WaitAsync(token);
    }

    /// <summary>
    /// An extension method to perform an action on a collection of items in parallel.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ForEach<T>(
        this Concurrent<T> concurrentReference,
        IAction<T> action) {
        if (concurrentReference.Source.Count is 0) {
            return;
        }
        Parallel.ForEach(concurrentReference.Source, action.Invoke);
    }

    /// <summary>
    /// An extension method to perform an action on a collection of items in parallel asynchronously and in batches.
    /// </summary>
    /// <param name="concurrentReference">The reference to the concurrent instance</param>
    /// <param name="action">the async action object</param>
    /// <param name="degreeOfParallelism">sets the number of tasks per batch</param>
    /// <param name="token">a cancellation token</param>
    /// <remarks>
    /// <para>If <paramref name="degreeOfParallelism"/> is set to -1, number of tasks per batch will be equal to the number of cores in the CPU</para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static Task ForEachAsync<T>(
        this Concurrent<T> concurrentReference,
        IAsyncAction<T> action,
        int degreeOfParallelism = -1,
        CancellationToken token = default) {
        if (concurrentReference.Source.Count is 0) {
            return Task.CompletedTask;
        }

        if (degreeOfParallelism is -1) {
            degreeOfParallelism = Environment.ProcessorCount;
        }

        var batchCount = concurrentReference.Source.Count <= degreeOfParallelism
            ? 1
            : concurrentReference.Source.Count / degreeOfParallelism;

        async Task AwaitPartition(IEnumerator<T> partition) {
            using (partition) {
                while (partition.MoveNext()) {
                    await action.InvokeAsync(partition.Current);
                }
            }
        }

        return Task.WhenAll(Partitioner
            .Create(concurrentReference.Source)
            .GetPartitions(batchCount)
            .AsParallel()
            .Select(AwaitPartition))
            .WaitAsync(token);
    }
}