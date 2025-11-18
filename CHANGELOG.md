# CHANGELOG

* Only .NET 10+ support.
* Some computed properties in `BufferWrapper{T}` were made `readonly` to hint the compiler to not create defensive copies.
* `PersistentDictionary` in all of its variants have been removed - used `ArrowDb` instead.
* `SerializableObject<T>`, `MonitoredSerializableObject<T>` and `ThreadSafe<T>` have all been combined into `Synchronized<T>` which is much simpler, more performant. But can be more manual as it isn't coupled with `File.IO`. Instead it provides a delegate that can be provided and called on update.
* `Collections.IsNullOrEmpty` and all the other alias functions for `CollectionsMarshal` have been removed.
* `Utils` subclasses have been flattened and all the contents will reside directly in `Utils`.
  * `Strings.FormatBytes` have been removed
  * `Path` utilities were also removed - use `AppContext` instead.
  * `String.IsNullOrEmpty|IsNullOrWhiteSpace|Concat` were also removed to enforce build-in language features.
  * `TryConvertFromTo32` was also removed - `int.Parse|TryParse` are more than fast enough now.
* A struct `PooledArrayOwner{T}` was added to rent arrays from `ArrayPool{T}` without additional penalties.
  * Extensions to match were added to any `ArrayPool{T}` including `Shared`, they allow you to get the owner and the array at the same time. `using var owner = ArrayPool{T}.Shared.Rent(minLength, out T[] array);`
  * You can then proceed to use `BufferWrapper{T}.Create(array)` to get an `IBufferWriter` implementation over it.
