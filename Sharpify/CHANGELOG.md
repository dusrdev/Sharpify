# CHANGELOG

## v1.2.0

* Modifications to `PersistentDictionary` (Some are *breaking changes*):
  * `Upsert` has been renamed to `UpsertAsync` to make its nature more obvious (Possible *BREAKING* change)
  * `GetOrCreateAsync(key, val)` and `UpsertAsync(key, val)` now return a `ValueTask` reducing resource usage
  * `PersistentDictionary` now uses a regular `Dictionary` as the internal data structure to be lighter and handle reads even faster, and a `ConcurrentQueue` is used internally to handle concurrent writes very efficiently with a more robust and consistent performance than before. This is the *BREAKING* change as custom inherited types will need to be updated to also serialize and deserialize to a regular `Dictionary`.
  * The base `Dictionary` is also not nullable anymore, which reduces null checks.
  * More methods of `PersistentDictionary` that had a base implementation were marked as `virtual` for more customization options with inheritance.
  * Overloads for `T` types were added to both `GetOrCreateAsync(key, T val)` and `UpsertAsync(key, T val)` to make usage even easier for primitive types, and they both rely on the `string` overloads so that inherited types would'nt need to implement both.
  * `LocalPersistentDictionary` and `LazyLocalPersistentDictionary` were both updated to support this new structure and also now utilize a single internal instance of the `JsonOptions` for serialization, thus reducing resource usage in some scenarios.
  * Edge cases of concurrent writing with `PersistentDictionary` are very hard to detect in unit tests due to inconsistencies in executing upserts in parallel, if you encounter any issues, please post the issue in the repo or email me.
* Added `OpenLink(string url)` function to `Utils.Env` that supports opening a link on Windows, Mac, and Linux
* `Result.Message` and `Result<T>.Message` is no longer nullable, and instead will default to an empty string.
* Added `object.TryUnbox<T>` and `string.GetReference` extensions
* Added `Result.Fail` overloads that support a value, to allow usage of static defaults or empty collections

## v1.1.0

* Changed nullability of return type of `PersistentDictionary.GetOrCreateAsync(key, val)`
* Finalized features, seems it is a good place to end the patches and start the next minor release

## v1.0.9

* Updated to support .NET 8.0
* Added `GetOrCreateAsync(key, val)` method to `PersistentDictionary`
* Further performance improvement to the `FormatBytes` extension
* `Either`s default empty constructor now throws an exception instead of simply warning during usage.
* `AesProvider.IsPasswordValid` was further optimized using spans (*Only applies when running > .NET8*)
* Updated outdates summary documentations

## v1.0.8

* Added 2 new persistent dictionary types: `LocalPersistentDictionary` and `LazyLocalPersistentDictionary`
  * Both of them Inherit from `PersistentDictionary`, they are essentially a `ConcurrentDictionary<string, string>` data store, which is optimized for maximal performance.
  * `LocalPersistentDictionary` requires a local path and utilizes Json to serialize and deserialize the dictionary, requiring minimal setup.
  * `LazyLocalPersistentDictionary` is similar to `LocalPersistentDictionary` but doesn't keep a permanent copy in-memory. Instead it loads and unloads the dictionary per operation.
  * Do not be mistaken by the simplicity of the `ConcurrentDictionary<string, string>` base type, as the string value allows you as much complexity as you want. You can create entire types for the value and just pass their to the dictionary.
  * `PersistentDictionary` is an abstract class which lays the ground work for creating such dictionaries with efficiency and thread-safety. You can create your own implementation easily by inheriting the class, you will need at the very least to override `SerializeAsync` and `Deserialize` and create your own constructors for setup. It is also possible to override `GetValueByKey` and `SetKeyAndValue` which allows you to implement lazy loading for example. The flexibility of the serialization is what gives you the option to persist the dictionary to where ever you choose, even an online database. For examples just look how `LocalPersistentDictionary` and `LazyLocalPersistentDictionary` are implemented in the source code.
  * Both types support a `StringComparer` parameter allowing you to customize the dictionary key management protocol, perhaps you want to ignore case, this is how you configure it.
* Added new extension method `ICollection<T>.IsNullOrEmpty` that check if it is null or empty using pattern matching.
* Added new function `Utils.Env.PathInBaseDirectory(filename)` that returns the combined path of the base directory of the executable and the filename.

## v1.0.7

* Performance increase in `RollingAverage` and `FibonacciApproximation`
* changes to `List.RemoveDuplicates`:
  * api change: parameter `isSorted` was moved to be after the `comparer` override, since it usually is used less frequently.
  * Another overload is available which accepts an `out HashSet<T>` parameter that can return the already allocated `HashSet` that was used to check the collection. Using it with `isSorted = true` is discouraged as the algorithm doesn't use n `HashSet` in that case, and it would be more efficient to just `new HashSet(list)` in that case.
* Small performance and stability enhancement in `DateTime.ToTimeStamp`
* `Concurrent.InvokeAsync` memory usage further optimized when using large collections by using an array with exact item count.
* Added new `Routines` namespace that includes two types: `Routine` and `AsyncRoutine`
  * Both types allow you to create a routine/background job that will execute a series of functions on a requested interval. And both support configuration with the `Builder` pattern.
  * `Routine` is the simplest and lightest that works best with simple actions.
  * `AsyncRoutine` is more complex and made specifically to accommodate async functions, it manages an async timer that will execute a collection of async functions. It has a `CancellationTokenSource` that will manage the cancellation of the timer itself and each of the functions. If you want more control you can provide it yourself. Despite the fact that `AsyncRoutine` can be configured using the `Builder` pattern, unlike `Routine`, the `Start` method returns a task, so to avoid loosing track of the routine, **DO NOT** call `Start` in the same call to the configuration.
  * `RoutineOptions` is an enum that is accepted to configure an `AsyncRoutine` and currently has 2 options:
    1. `ExecuteInParallel` this will create execute the functions provided in parallel in every tick, this may increase memory allocation since parallel execution requires a collection of tasks to be re-created upon every execution. But, it might provide a speed benefit when using long-running background functions in the routine.
    2. `ThrowOnCancellation`, stopping a task using a `cancellationToken` inevitably throws a `TaskCancelledException`. By default to make the routine easier to use it ignores this exception as it should only occur by design. If you toggle this option, it will re-throw the exception and you will be required to handle it. If you want to ensure that the routine finishes when you want to without controlling the token, simply call `Dispose` on the routine.
* New collection type `SortedList<T>` in `Sharpify.Collections`, it is a List that maintains a sorted order, unlike the original `SortedList<K,V>` which is based on a sorted dictionary and a tree. This isn't, it is lighter, more customizable. And enables super efficient iteration and even internal `Span<T>` access
  * Performance stats:
  * Initialization from collection: O(nlogn)
  * Add and remove item: O(logn)
  * Get by index: O(1)

## v1.0.6

* New `RemoveDuplicates` extensions for `List<T>` was added, it is implemented using a more efficient algorithm, and has an optional parameter `isSorted` that allows further optimization. There is also an optional `IEqualityComparer<T>` parameter for types that their default comparer doesn't provide accurate results
* Performance enhancements to `AesProvider`
* New `ChunkToSegments` extension method for arrays that returns `List<ArraySegment>>` that can be used to segment array and process them concurrently, which is the most efficient way to do this as `Enumerable.Chunk` will actually create new arrays (much more memory allocation), and `span`s are very limited when it comes to concurrent processing with `Task`s.
* Optimized `base64` related methods in `AesProvider`
* Several methods that relied on exception throwing for invalid input parameters, no instead use `Debug.Assert` to improve performance on Release builds.

### Breaking Changes

* `List` extensions: `RemovedDuplicatesSorted`, `SortAndRemoveDuplicates` have been removed. Their functionality is replaces with the new `RemoveDuplicates` extension

## v1.0.5

THIS UPDATE MAY INTRODUCES THE FOLLOWING BREAKING CHANGES BUT THEY ARE REQUIRED FOR FURTHER STABILITY

* Updated `Result` and `Result<T>` to disallow default constructors, thus enforcing use of factory methods such as `Result.OK(message)` and `Result.Fail(message)` and their overloads.
* Updated `Concurrent` to also disallow default constructor, enforcing use of `ICollection.Concurrent()` extension method.

## v1.0.4

* Added url encryption and decryption functions to `AesProvider`
* Added more safeguards that prevent exceptions when `plain` text input is passed as `encrypted` to decryption functions. They now return an empty output, either `byte[]` or `string.Empty`, depends on the method.

## v1.0.3

* Fixed implementation of `IModifier<T>` to better fit the requirements of `Func<T, T>`

## v1.0.2

* Introduces a new `ThreadSafe<T>` wrapper which makes any type thread-safe
* Introduces a new `AesProvider` class with enables no-setup access to encryption

### `ThreadSafe<T>` Notes

* Access the value by using `ThreadSafe<T>.Value`
* Modify the value by using `ThreadSafe<T>.Modify(Func<T, T>)` or `ThreadSafe<T>.Modify(IModifier<T>)`
* `IModifier<T>` allows you to create an operation that will be better optimized than a `Func<T, T>` and possibly avoid the memory allocation penalty associated with `delegates`
* Unlike the `Interlocked` api's, which require using `ref` thus making them unusable in `async` methods, `ThreadSafe<T>` doesn't, which makes it much more usable

### `AesProvider` Notes

* Has `string` key based constructor that takes care of the headache for padding and key size for you
* Has methods for encrypting and decrypting both `string`s and `byte[]`s
* Provides an option to generate an `ICryptoTransform` for either encryption or decryption to fit special needs
* Properly implements `IDisposable` with api notices to prevent memory leaks
* Has static methods for generating hashed passwords and validating them

## v1.0.1

* `Utils` class was made upper class of `Env`, `Mathematics` and `DateAndTime` to allow better categorization and maintenance
* Fixed invalid options in the .csproj file

### `Utils.Env`

* Added `GetBaseFolder` which returns the base path of the application directory
* Added `IsRunningAsAdmin` and `IsRunningOnWindows` which are self-explanatory
* Added `IsInternetAvailable` which checks for internet connection

### `Utils.Mathematics`

* Added `FibonacciApproximation` and `Factorial` functions
