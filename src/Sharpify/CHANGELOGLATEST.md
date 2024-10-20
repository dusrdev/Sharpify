# CHANGELOG

## v2.4.0

* All derived types of `PersistentDictionary` now implement `IDisposable` interface.
* Main concurrent processing methods are now `ICollection<T>.ForAll()` and `ICollection<T>.ForAllAsync()` from many, many benchmarks it became clear that for short duration tasks not involving heavy compute, it has by far the best compromise of speed and memory-allocation. If you use it with a non `async function` all the tasks will yield immediately and require virtually no allocations at all. Which is as good as `ValueTask` from benchmarks. This method has 2 overloads, one which accepts an `IAsyncAction` which enables users with long code and many captured variables to maintain a better structured codebase, and a `Func` alternative for quick and easy usage. The difference in memory allocation / execution time between time is nearly non-existent, this mainly for maintainability.
* For heavier compute tasks, please revert to using `Parallel.For` or `Parallel.ForEachAsync` and their overloads, they are excellent in load balancing.
* Due to the changes above, all other concurrent processing methods, such as `ForEachAsync`, `InvokeAsync` and all the related functionality from the `Concurrent` class have been removed. `AsAsyncLocal` entry is also removed, and users will be able to access the new `ForAll` and `ForAllAsync` methods directly from the `ICollection<T>` interface static extensions.
* `ForAll` and `ForAllAsync` methods have identical parameters, the only difference is that the implementation for `ForAll` is optimized for synchronous lambdas that don't need to allocate an AsyncStateMachine, while the `ForAllAsync` is optimized for increased concurrency for asynchronous lambdas. Choosing `ForAll` for synchronous lambdas massively decreases memory allocation and execution time.
* `IAsyncAction<T>`'s `InvokeAsync` method now has a `CancellationToken` parameter.
* Changes to `TimeSpan` related functions:
  * `Format`, `FormatNonAllocated`, `ToRemainingDuration`, `ToRemainingDurationNonAllocated`, `ToTimeStamp`, `ToTimeStampNonAllocated`, were all removed due to duplication and suboptimal implementations.
  * The new methods replacing these functionalities are now in `Utils.DateAndTime` namespace.
  * `FormatTimeSpan` is now replacing `Format` and `FormatNonAllocated`, `FormatTimeSpan` is hyper optimized. The first overload requires a `Span{char}` buffer of at least 30 characters, and returns a `ReadOnlySpan{char}` of the written portion. The second doesn't require a buffer, and allocated a new `string` which is returned. `FormatTimeSpan` outputs a different format than the predecessor, as the time was formatted in decimal and is rather confusing, now it is formatted as `00:00unit` for the largest 2 units. So a minute and a half would be `01:30m` and a day and a half would be `02:30d` etc... this seems more intuitive to me.
  * `FormatTimeStamp` is now replacing `ToTimeStamp` and `ToTimeStampNonAllocated`, it is also optimized and the overloads work the same way as `FormatTimeSpan`.
* The `StringBuffer` which previously rented arrays from the shared array pool, then used the same API's to write to it as `AllocatedStringBuffer` was removed. The previous `AllocatedStringBuffer` was now renamed to `StringBuffer` and it requires a pre-allocated `Span{char}`. You can get the same functionality by renting any buffer, and simply supplying to `StringBuffer.Create`. This allowed removal of a lot of duplicated code and made the API more consistent. `StringBuffer` now doesn't have an implicit converter to `ReadOnlySpan{char}` anymore, use `StringBuffer.WrittenSpan` instead.
* `IModifier{T}` was removed, use `Func<T, T>` instead.
* `Utils.Strings.FormatBytes` was changed in the same manner as `Utils.DateAndTime.FormatTimeSpan` and `Utils.DateAndTime.FormatTimeStamp`, it now returns a `ReadOnlySpan<char>` instead of a `string` and it is optimized to use less memory.
* `ThreadSafe<T>` now implements `IEquatable<T>` and `IEquatable<ThreadSafe<T>>` to allow comparisons.

Upon the release of .NET 9, another version will be released utilizing certain optimizations specifically for .NET 9, but the feature set should be the same.
