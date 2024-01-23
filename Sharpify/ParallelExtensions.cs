using System.Buffers;
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
    /// Returns a <see cref="AsyncLocal{T}"/> wrapper for the given collection.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AsyncLocal<IList<T>> AsAsyncLocal<T>(this IList<T> source) => source is null
        ? throw new ArgumentNullException(nameof(source))
        : new AsyncLocal<IList<T>> {
            Value = source
        };

    /// <summary>
    /// An extension method to perform an action on a collection of items in parallel.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static Task InvokeAsync<T>(
        this Concurrent<T> concurrentReference,
        IAsyncAction<T> action,
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
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static async Task InvokeAsync<T>(
        this AsyncLocal<IList<T>> asyncLocalReference,
        IAsyncAction<T> action,
        CancellationToken token = default) {
        ArgumentNullException.ThrowIfNull(asyncLocalReference.Value);
        var length = asyncLocalReference.Value.Count;

        if (length is 0) {
            return;
        }

        var array = ArrayPool<Task>.Shared.Rent(length);
        var tasks = new ArraySegment<Task>(array, 0, length);

        try {
            var i = 0;
            foreach (var item in asyncLocalReference.Value) {
                tasks[i++] = action.InvokeAsync(item);
            }
            await Task.WhenAll(tasks).WaitAsync(token).ConfigureAwait(false);
        } finally {
            array.ReturnBufferToSharedArrayPool();
        }
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

        var enumeratedPartition = new EnumeratedPartition<T>(action);

        return Task.WhenAll(Partitioner
            .Create(concurrentReference.Source)
            .GetPartitions(batchCount)
            .AsParallel()
            .Select(enumeratedPartition.AwaitPartitionAsync))
            .WaitAsync(token);
    }

    /// <summary>
    /// An extension method to perform an action on a collection of items in parallel asynchronously and in batches.
    /// </summary>
    /// <param name="asyncLocalReference">The reference to the async local instance that holds the collection</param>
    /// <param name="action">the async action object</param>
    /// <param name="degreeOfParallelism">sets the number of tasks per batch</param>
    /// <param name="loadBalance"></param>
    /// <param name="token">a cancellation token</param>
    /// <remarks>
    /// <para>If <paramref name="degreeOfParallelism"/> is set to -1, number of tasks per batch will be equal to the number of cores in the CPU</para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static async Task ForEachAsync<T>(
        this AsyncLocal<IList<T>> asyncLocalReference,
        IAsyncAction<T> action,
        int degreeOfParallelism = -1,
        bool loadBalance = false,
        CancellationToken token = default) {
        ArgumentNullException.ThrowIfNull(asyncLocalReference.Value);
        var count = asyncLocalReference.Value.Count;

        if (count is 0) {
            return;
        }

        if (degreeOfParallelism is -1) {
            degreeOfParallelism = Environment.ProcessorCount;
        }

        var batchCount = count <= degreeOfParallelism
            ? 1
            : count / degreeOfParallelism;

        var enumeratedPartition = new EnumeratedPartition<T>(action);

        var parallelPartitions = Partitioner
            .Create(asyncLocalReference.Value, loadBalance)
            .GetPartitions(batchCount)
            .AsParallel();

        var array = ArrayPool<Task>.Shared.Rent(batchCount);
        var partitions = new ArraySegment<Task>(array, 0, batchCount);

        try {
            var i = 0;
            foreach (var partition in parallelPartitions) {
                partitions[i++] = enumeratedPartition.AwaitPartitionAsync(partition);
            }
            await Task.WhenAll(partitions).WaitAsync(token).ConfigureAwait(false);
        } finally {
            array.ReturnBufferToSharedArrayPool();
        }
    }

    /// <summary>
    /// An extension method to perform an value action on a collection of items in parallel asynchronously.
    /// </summary>
    /// <param name="asyncLocalReference">The reference to the async local instance that holds the collection</param>
    /// <param name="action">the async value action object</param>
    /// <param name="token">a cancellation token</param>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static async ValueTask WhenAllAsync<T>(
        this AsyncLocal<IList<T>> asyncLocalReference,
        IAsyncValueAction<T> action,
        CancellationToken token = default) {
        ArgumentNullException.ThrowIfNull(asyncLocalReference.Value);
        var count = asyncLocalReference.Value.Count;

        if (count is 0) {
            return;
        }

        var totalArray = ArrayPool<ValueTask>.Shared.Rent(count);
        var totalSegment = new ArraySegment<ValueTask>(totalArray, 0, count);
        int totalIndex = 0;
        var needAllocation = ArrayPool<Task>.Shared.Rent(count);
        int needAllocationIndex = 0;

        try {
            foreach (var item in asyncLocalReference.Value) {
                totalSegment[totalIndex++] = action.InvokeAsync(item);
            }
            foreach (var valueTask in totalSegment) {
                if (valueTask.IsCompletedSuccessfully) {
                    continue;
                }
                needAllocation[needAllocationIndex++] = valueTask.AsTask();
            }
            var requiringAllocation = new ArraySegment<Task>(needAllocation, 0, needAllocationIndex);
            if (requiringAllocation.Count is 0) {
                return;
            }
            await Task.WhenAll(requiringAllocation).WaitAsync(token).ConfigureAwait(false);
        } finally {
            totalArray.ReturnBufferToSharedArrayPool();
            needAllocation.ReturnBufferToSharedArrayPool();
        }
    }
}