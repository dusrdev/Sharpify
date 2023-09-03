# CHANGELOG

## v1.0.7

* Performance increase in `RollingAverage` and `FibonacciApproximation`
* `List.RemoveDuplicates` api change: parameter `isSorted` was moved to be after the `comparer` override, since it usually is used less frequently.
* Small performance and stability enhancement in `DateTime.ToTimeStamp`

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
