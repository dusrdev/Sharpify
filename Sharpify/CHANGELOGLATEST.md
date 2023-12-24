# CHANGELOG

* Modifications to `PersistentDictionary`:
  * `Upsert` has been renamed to `UpsertAsync` to make its nature more obvious (Possible **BREAKING** change)
  * `Upsert` now handles a special case in which the key exists and value is the same as new value, it will completely forgo the operation, requiring no `Task` creation and no serialization.
  * `GetOrCreateAsync(key, val)` and `UpsertAsync(key, val)` now return a `ValueTask` reducing resource usage
  * `PersistentDictionary` now uses a regular `Dictionary` as the internal data structure to be lighter and handle reads even faster. This is the ***BREAKING** change as custom inherited types will need to be updated to also serialize and deserialize to a regular `Dictionary`.
  * To allow concurrent writes, a very efficient and robust concurrency model using a `ConcurrentQueue` and a `SemaphoreSlim` is used. It perfect conditions it will even reduce serialization counts.
  * The base `Dictionary` is also not nullable anymore, which reduces null checks.
  * More methods of `PersistentDictionary` that had a base implementation were marked as `virtual` for more customization options with inheritance.
  * Overloads for `T` types were added to both `GetOrCreateAsync(key, T val)` and `UpsertAsync(key, T val)` to make usage even easier for primitive types, and they both rely on the `string` overloads so that inherited types would'nt need to implement both.
  * `LocalPersistentDictionary` and `LazyLocalPersistentDictionary` were both updated to support this new structure and also now utilize a single internal instance of the `JsonOptions` for serialization, thus reducing resource usage in some scenarios.
  * `LazyLocalPersistentDictionary` get key implementation was revised and improved to reduce memory allocations.
  * Edge cases of concurrent writing with `PersistentDictionary` are very hard to detect in unit tests due to inconsistencies in executing upserts in parallel, if you encounter any issues, please post the issue in the repo or email me.
* Added `OpenLink(string url)` function to `Utils.Env` that supports opening a link on Windows, Mac, and Linux
* `Result.Message` and `Result<T>.Message` is no longer nullable, and instead will default to an empty string.
* `string.GetReference` extension
* Added `Result.Fail` overloads that support a value, to allow usage of static defaults or empty collections
* Added `HashSet.ToArrayFast()` method which converts a hash set to an array more efficiently than Linq.
* Further optimized `AesProvider.GeneratePassword`
* **BREAKING**, The `FormatBytes` function for `long` and `double` was moved to `Utils.Strings` class and is no longer an extension function, this will make the usage clearer.
* Further optimized `TimeSpan.Format`
* Multiple string creating functions which used to stack allocate the buffers, now rent them instead, potentially reducing overall application memory usage.
* Added another class `Utils.Unsafe` that has "hacky" utilities that allow you to reuse existing code in other high performance apis
* New exceptions were added to validate function input in places where the JIT could you use this information to optimize the code by removing bound checks and such.
* **BREAKING** all of the `ConvertToInt32` methods were removed, an in place a method `TryConvertToInt32(ReadOnlySpan{char}, out int result)` was added, it is more efficient and generic as it can work for signed and unsigned integers by relaying on the bool as the operation status.
* `SortedList<T>`'s fields were changed to be protected and not private, this will make inheritance if you so choose, also added an implicit operator that will return the inner list for places which require a list input.
