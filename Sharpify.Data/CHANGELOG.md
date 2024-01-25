# CHANGELOG

## v2.1.0

* Small **breaking** change in `DatabaseFilter{T}`, the filter will now use a `:` delimiter between the type name and the key, this means you keys won't be found if were upserted using pre-change filters. This is unfortunate but necessary change in order to 1. enable better filtering of keys from the `Database`, enabling searching and using split to get the second portion of the key. and 2. to lay the groundwork of possibly implementing more filters in the future.

* `DatabaseFilter{T}` type was changed from `readonly struct` to `class`, and it now implements the `IDatabaseFilter{T}` interface. the internal `CreateKey` that in the default implementation uses the type name and `:` to create a "filtered" key, is now marked as virtual. So that it is possible to inherit from `DatabaseFilter{T}` and override `CreateKey` to either use a different template, or even add to it, for example if you have nested generics, such as `TMemoryPackable<TOther>`, in which case the default `DatabaseFilter{T}` would not be able do distinguish between the inner generic, possibly causing issues with serialization and deserialization. The change to `class` also should be costly, as the database filter can be stored as a field as well, and used similarly to `dbContext` of other databases. In case overriding `CreateKey` is not enough, you can of course implement the whole `IDatabaseFilter{T}` interface if you so choose.

`DatabaseFilter{T}` was initially designed better in the context of APIs, no longer offering the `Get`, but instead using `TryGetValue`, while it might require 1 line of code more, when reading and writing the code, it is less ambiguous, before, a null or default result could indicate `not found`, `failed to deserialized`, and even upserted as null. Now if `TryGetValue` returns false there can only be one reason and that is that the key did not exist.

* To improve this conciseness, `Database` now has `TryGetValue` offerings, for regular `ReadOnlyMemory{byte}` output, `IMemoryPackable{T}` and `string`. These are now the preferred APIs to use when retrieving values.
* The old `Get` variants of `Database` are now marked as `Deprecated` to signal they shouldn't be used. This was made to reduce the amount of breaking changes in this version, the `Get` variants will stay on as `Deprecated` until the next `Major` version, at which point they will be deleted. I hope this gives you enough time to "migrate".

* `UpsertAsString` and `UpsertAsT`(JSON version), are now also named just `Upsert`, their overload is inferred from the type of the arguments as string is not `IMemoryPackable{T}` and the JSON `T` version requires a `JsonSerializerContext`.
* Also added `TryGetValue` overloads for JSON `T`, you will know them because they both require a `JsonSerializerContext`. apparently before this version you could only `Upsert` a JSON `T`, I apologize for the oversight.

## v2.0.2

* Moved `_lock` acquiring statements into the `try-finally` blocks to handle `out-of-band` exceptions. Thanks [TheGenbox](https://www.reddit.com/user/TheGenbox/).
* Modified all internal `await` statements to use `ConfigureAwait(false)`, also thanks to [TheGenbox](https://www.reddit.com/user/TheGenbox/).

## v2.0.1

* Added `ContainsKey` and `Remove` functions to `DatabaseFilter{T}` to give it essentially the same functionality scope as the `Database` itself.
* Removed `Flags` attribute from `DataChangeType` as it wasn't really a flag type, it can only be one thing.

## v2.0.0 - BREAKING CHANGE

The entire package has been reworked.

* `Database{T}` was removed, and so was `DatabaseOptions`
* `Database` is now the only offer, and was tremendously optimized.
* GC pressure massively reduced due to very heavy usage of pooling.
* The options that were previously part of `DatabaseOptions` enum are now simple `bool`s in `DatabaseConfiguration`
* `string` value type upsert and get now uses `MemoryPack` for improved efficiency.
* the base value type is now a `ReadOnlyMemory<byte>` instead of `byte[]` enabling internal use of better apis and more optimization, client usage shouldn't change much because of implicit converters.
* `DatabaseFilter{T}` is a `readonly struct` filter of the `Database` based on a types that are `IMemoryPackable`, the filter should provide an AOT-friendly way to use multiple types in the same file, while ensuring no unforeseen `deserialization` issues occur because of key exists but with value of different type. `DatabaseFilter{T}` is simple and AOT-friendly because it does no changes to `Database` whatsoever, instead just wraps the key that it uses with a modification that includes the name of the type. It is only an abstraction. Nevertheless, this abstraction is very powerful as it takes no additional effort from the user, and allowing the user to create generic consumer classes, all of which use the same database, but injecting the filter instead of the database, making it virtually impossible that a generic class will try to use a value of a different type.
* Using `DatabaseFilter{T}` without per key encryption should perform on par with `Database{T}`, which allowed its removal due to deprecation.
* `DatabaseFilter{T}` is a lightweight struct that won't have a big performance impact when it is copied by value, but it is possible to negate even that by using the `ref` and `in` keywords in your own APIs
* An internal abstraction of `DatabaseSerializer`, which has multiple implementations that vary by the `DatabaseConfiguration`, is now created with the database initialization and reused, enabling both more efficient hot-paths for all serialization related functionality and usage of code that is specifically optimized per each variation of `DatabaseConfiguration`.
* More optimizations were made to allow JIT to branch eliminate parts of the manual `Serialize` and `SerializeAsync` based on the `DatabaseConfiguration`.
* Lower level APIs are now used internally to interact with the underlying data structure that should shave a few nanoseconds out of the CRUD operations.

The deletion of `Database{T}` gave way to make `Database` a much greater and more customizable database, all the previous functionality was improved, and the usage `DatabaseFilter{T}` without per-key encryption, now offers a replacement to `Database{T}` which is more extensible to allow usage for multiple `{T}` types, where as `Database{T}` allowed a single type.

The new changes mean that runtime deserialization exceptions are almost guaranteed if you try to use it on a pre-existing database. It is recommended to either start from fresh (Which is not a big issue if you used the database for caching), or perform some matching (the easiest of which will probably be to extract all the values using the `GetKeys` and `Get` functions, save them somewhere and delete the database, then upgrade, initialize a database with similar configuration and add all the values back)

## v1.1.0

* **BREAKING** `UpsertAsT` functions signature changed to force usage of new parameter, which is a `JsonSerializerContext` that can serialize `T`.
* Simplified structures of inner data representation, slightly reducing assembly size.
* `Sharpify.Data` is now fully AOT compatible

## v1.0.3

* Upgraded concurrency synchronization model of `Database` and `Database{T}` to get more accurate reads when other threads are writing.
* Both now implement `IDisposable` to release the resources of the synchronization and should be disposed of properly, but since they are designed to be used throughout the lifetime of the application, it isn't absolutely crucial to do this, and the implement finalizer should take care of this, if disposing of it from your end is inconvenient

## v1.0.2

* Fixed issue where an exception would be thrown if `Upsert` overrides a key. it should by design override.

## v1.0.1

* Further optimized both database, heavily utilizing array pooling for encryption

## v1.0.0

* Initial release - check github for information
