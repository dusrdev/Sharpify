using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Sharpify;

public static partial class Extensions {
    /// <summary>
    /// Returns a <see cref="Concurrent{T}"/> wrapper for the given collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Concurrent<T> Concurrent<T>(this ICollection<T> source) => source is null
        ? throw new ArgumentNullException(nameof(source))
        : new Concurrent<T>(source);

    /// <summary>
    /// An extension method to perform an action on a collection of items in parallel.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="concurrentReference"></param>
    /// <param name="action"></param>
    /// <param name="token"></param>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static Task InvokeAsync<T>(
        this Concurrent<T> concurrentReference,
        in IAsyncAction<T> action,
        CancellationToken token = default) {
        var tasks = new Task[concurrentReference.Source.Count];
        var i = 0;
        foreach (var item in concurrentReference.Source) {
            tasks[i++] = action.InvokeAsync(item);
        }
        return Task.WhenAll(tasks).WaitAsync(token);
    }

    /// <summary>
    /// An extension method to perform an action on a collection of items in parallel.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="concurrentReference"></param>
    /// <param name="action"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ForEach<T>(
        this Concurrent<T> concurrentReference,
        IAction<T> action) {
        Parallel.ForEach(concurrentReference.Source, action.Invoke);
    }

    /// <summary>
    /// An extension method to perform an action on a collection of items in parallel asynchronously and in batches.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="concurrentReference"></param>
    /// <param name="action"></param>
    /// <param name="degreeOfParallelization">sets the number of tasks per batch</param>
    /// <param name="token"></param>
    /// <remarks>If <paramref name="degreeOfParallelization"/> is set to -1, batch size will be equal to the number of cores in the CPU</remarks>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static Task ForEachAsync<T>(
        this Concurrent<T> concurrentReference,
        IAsyncAction<T> action,
        int degreeOfParallelization = -1,
        CancellationToken token = default) {
        if (concurrentReference.Source.Count is 0) {
            return Task.CompletedTask;
        }

        if (degreeOfParallelization is -1) {
            degreeOfParallelization = Environment.ProcessorCount;
        }

        int batchCount = concurrentReference.Source.Count <= degreeOfParallelization
            ? 1
            : concurrentReference.Source.Count / degreeOfParallelization;

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