# CHANGELOG

## v1.7.0

* Added `AppendLine` overloads to all `Append` methods of the `StringBuffer` variants that append the platform specific new line sequence at after the `Append`, there is also an empty overload which just appends the new line sequence. This was made to reduce code when using newlines, and to make the api even more similar to `StringBuilder`.
* Added `RentedBufferWriter{T}` to `Sharpify.Collections`, it is an implementation of the `IBufferWriter{T}` that uses an array rented from the shared array pool as the backing buffer, it provides an allocation free alternative to `ArrayBufferWriter{T}`.
* Heavily implement array pooling in `AesProvider`, increasing performance and reducing memory allocations in virtually all APIs.
* Added another `CopyToArray` extension for `HashSet{T}` which enables usage of pre-existing buffer, which in turn enables usage of pooling.
* Improved performance of `LazyPersistentDictionary` reads using pooling.
* Reduced memory usage in initialization of `SerializableObject{T}`
* `AsyncRoutine` with the option `ExecuteOnParallel` now also uses pooling, virtually eliminating memory allocations (which used to happen regularly per the execution interval)
* Optimized `FibonacciApproximation` in `Utils.Mathematics`
