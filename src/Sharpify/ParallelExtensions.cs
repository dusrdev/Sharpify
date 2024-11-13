using Sharpify.Collections;

namespace Sharpify;

public static partial class Extensions {
    /// <summary>
    /// An extension method to perform an action on a collection of items in parallel.
    /// </summary>
    /// <param name="collection">The source collection.</param>
    /// <param name="body">The action to be performed.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task that completes when the entire <paramref name="collection"/> has been processed.</returns>
    /// <remarks>
    /// <para>If <paramref name="collection"/> is null or empty, the task will return immediately</para>
    /// <para>The cancellation token will be injected into <paramref name="body"/> for each item of the collection</para>
    /// <para>Unlike <see cref="ForAllAsync{T}(ICollection{T}, Func{T, CancellationToken, Task}, CancellationToken)"/>, this method is optimized for synchronous lambdas that don't need to allocate an AsyncStateMachine</para>
    /// </remarks>
    public static async Task ForAll<T>(
        this ICollection<T>? collection,
        Func<T, CancellationToken, Task> body,
        CancellationToken token = default) {
        if (collection is null or { Count: 0 }) {
            return;
        }

        var length = collection.Count;

        using var taskBuffer = new RentedBufferWriter<Task>(length);

        foreach (var item in collection) {
            taskBuffer.WriteAndAdvance(body.Invoke(item, token));
        }

#if NET9_0_OR_GREATER
        await Task.WhenAll(taskBuffer.WrittenSpan).WaitAsync(token).ConfigureAwait(false);
#else
        await Task.WhenAll(taskBuffer.WrittenSegment).WaitAsync(token).ConfigureAwait(false);
#endif
    }

    /// <summary>
    /// An extension method to perform an action on a collection of items in parallel.
    /// </summary>
    /// <param name="collection">The source collection.</param>
    /// <param name="asyncAction">The action to be performed.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task that completes when the entire <paramref name="collection"/> has been processed.</returns>
    /// <remarks>
    /// <para>If <paramref name="collection"/> is null or empty, the task will return immediately</para>
    /// <para>The cancellation token will be injected into <paramref name="asyncAction"/> for each item of the collection</para>
    /// <para>Unlike <see cref="ForAllAsync{T}(ICollection{T}, IAsyncAction{T}, CancellationToken)"/>, this method is optimized for synchronous lambdas that don't need to allocate an AsyncStateMachine</para>
    /// </remarks>
    public static Task ForAll<T>(
        this ICollection<T>? collection,
        IAsyncAction<T> asyncAction,
        CancellationToken token = default)
        => ForAll(collection, asyncAction.InvokeAsync, token);

    /// <summary>
    /// An extension method to perform an action on a collection of items in parallel.
    /// </summary>
    /// <param name="collection">The source collection.</param>
    /// <param name="body">The action to be performed.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task that completes when the entire <paramref name="collection"/> has been processed.</returns>
    /// <remarks>
    /// <para>If <paramref name="collection"/> is null or empty, the task will return immediately</para>
    /// <para>The cancellation token will be injected into <paramref name="body"/> for each item of the collection</para>
    /// </remarks>
    public static async Task ForAllAsync<T>(
        this ICollection<T>? collection,
        Func<T, CancellationToken, Task> body,
        CancellationToken token = default) {
        if (collection is null or { Count: 0 }) {
            return;
        }

        var length = collection.Count;

        using var taskBuffer = new RentedBufferWriter<Task>(length);

        foreach (var item in collection) {
            taskBuffer.WriteAndAdvance(Task.Run(() => body.Invoke(item, token), token));
        }

#if NET9_0_OR_GREATER
        await Task.WhenAll(taskBuffer.WrittenSpan).WaitAsync(token).ConfigureAwait(false);
#else
        await Task.WhenAll(taskBuffer.WrittenSegment).WaitAsync(token).ConfigureAwait(false);
#endif
    }

    /// <summary>
    /// An extension method to perform an action on a collection of items in parallel.
    /// </summary>
    /// <param name="collection">The source collection.</param>
    /// <param name="asyncAction">The action to be performed.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task that completes when the entire <paramref name="collection"/> has been processed.</returns>
    /// <remarks>
    /// <para>If <paramref name="collection"/> is null or empty, the task will return immediately</para>
    /// <para>The cancellation token will be injected into <paramref name="asyncAction"/> for each item of the collection</para>
    /// </remarks>
    public static Task ForAllAsync<T>(
        this ICollection<T>? collection,
        IAsyncAction<T> asyncAction,
        CancellationToken token = default)
        => ForAllAsync(collection, asyncAction.InvokeAsync, token);
}