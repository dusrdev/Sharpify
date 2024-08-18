using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

using Sharpify.Collections;

namespace Sharpify;

public static partial class Extensions {
    /// <summary>
    /// Returns a <see cref="AsyncLocal{T}"/> wrapper for the given collection.
    /// </summary>
    public static AsyncLocal<TList> AsAsyncLocal<TList, TItem>(this TList source) where TList : IList<TItem> {
        if (source is null) {
            throw new ArgumentNullException(nameof(source));
        }
        return new AsyncLocal<TList> {
            Value = source
        };
    }

    /// <summary>
    /// Returns a <see cref="AsyncLocal{T}"/> wrapper for the given collection.
    /// </summary>
    public static AsyncLocal<TList> AsAsyncLocal<TList, TItem>(this TList source, TItem? @default) where TList : IList<TItem> => AsAsyncLocal<TList, TItem>(source);

    /// <summary>
    /// An extension method to perform an action on a collection of items in parallel.
    /// </summary>
    public static async Task InvokeAsync<TList, TItem>(
        this AsyncLocal<TList> asyncLocalReference,
        IAsyncAction<TItem> action,
        CancellationToken token = default) where TList : IList<TItem> {
        ArgumentNullException.ThrowIfNull(asyncLocalReference.Value);
        var length = asyncLocalReference.Value.Count;

        if (length is 0) {
            return;
        }

        using var taskBuffer = new RentedBufferWriter<Task>(length);

        foreach (var item in asyncLocalReference.Value) {
            taskBuffer.WriteAndAdvance(action.InvokeAsync(item));
        }
        await Task.WhenAll(taskBuffer.WrittenSegment).WaitAsync(token).ConfigureAwait(false);
    }

    /// <summary>
    /// An extension method to perform an action on a collection of items in parallel.
    /// </summary>
    public static void ForEach<T>(
        this ICollection<T> collection,
        IAction<T> action) {
        if (collection.Count is 0) {
            return;
        }
        Parallel.ForEach(collection, action.Invoke);
    }

    /// <summary>
    /// An extension method to perform an action on a collection of items in parallel.
    /// </summary>
    /// <param name="asyncLocalReference">The reference to the async local instance that holds the collection</param>
    /// <param name="action">the action object</param>
    /// <param name="degreeOfParallelism">sets the baseline number of actions per iteration</param>
    /// <param name="token">a cancellation token</param>
    /// <remarks>
    /// <para>If <paramref name="degreeOfParallelism"/> is set to -1, it will be equal to the number of cores in the CPU</para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static ValueTask ForEach<TList, TItem>(
        this AsyncLocal<TList> asyncLocalReference,
        IAction<TItem> action,
        int degreeOfParallelism = -1,
        CancellationToken token = default) where TList : IList<TItem> {
        ArgumentNullException.ThrowIfNull(asyncLocalReference.Value);
        var count = asyncLocalReference.Value.Count;

        if (count is 0) {
            return ValueTask.CompletedTask;
        }

        if (degreeOfParallelism is -1) {
            degreeOfParallelism = Environment.ProcessorCount;
        }

        var partitioner = Partitioner.Create(asyncLocalReference.Value, true);

        var options = new ParallelOptions {
            MaxDegreeOfParallelism = degreeOfParallelism,
            CancellationToken = token
        };

        _ = Parallel.ForEach(partitioner, options, action.Invoke);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// An extension method to perform an value action on a collection of items in parallel asynchronously.
    /// </summary>
    /// <param name="asyncLocalReference">The reference to the async local instance that holds the collection</param>
    /// <param name="action">the async value action object</param>
    /// <param name="token">a cancellation token</param>
    public static async ValueTask WhenAllAsync<TList, TItem>(
        this AsyncLocal<TList> asyncLocalReference,
        IAsyncValueAction<TItem> action,
        CancellationToken token = default) where TList : IList<TItem> {
        ArgumentNullException.ThrowIfNull(asyncLocalReference.Value);
        var count = asyncLocalReference.Value.Count;

        if (count is 0) {
            return;
        }

        using var valueTaskBuffer = new RentedBufferWriter<ValueTask>(count);
        using var taskBuffer = new RentedBufferWriter<Task>(count);

        foreach (var item in asyncLocalReference.Value) {
            valueTaskBuffer.WriteAndAdvance(action.InvokeAsync(item));
        }
        foreach (var valueTask in valueTaskBuffer.WrittenSegment) {
            if (valueTask.IsCompletedSuccessfully) {
                continue;
            }
            taskBuffer.WriteAndAdvance(valueTask.AsTask());
        }
        if (taskBuffer.Position is 0) {
            return;
        }
        await Task.WhenAll(taskBuffer.WrittenSegment).WaitAsync(token).ConfigureAwait(false);
    }
}