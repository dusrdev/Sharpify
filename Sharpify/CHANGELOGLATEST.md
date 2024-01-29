# CHANGELOG

## v1.7.3

* Made improvements to all `IDisposable` implementing types making their `Dispose` implementations idempotent (consecutive calls will just be ignored, eliminating exceptions that can occur when misused).
* Added a new `AsyncLocal<IList<T>>` extension `ForEach` overload that uses a synchronous `IAction<T>` implementation, it should be the more modern approach to executing synchronous but parallel actions, it also has parameters for `degreeOfParallelism` and `cancellationToken`, and it also returns a `ValueTask`, which means you should await it, and you can attempt to cancel it if you want. It should also perform a bit better as now it is using a partitioner internally. In the case of performing synchronous operations on parallel, there is less room for speed optimization that can occur by jumping between tasks when a task is just waiting like it is possible in async, here your main tool to optimize will be the figure out the right amount of `degreeOfParallelism`, taking into account the capabilities of the system and complexity of each action. Nevertheless, the default (-1) is not a bad baseline.
  * If you want the call site to be sync as well, use the "older" `ForEach` `Concurrent` overload, as calling the new `ValueTask` overload in a non-async context, will degrade performance.
