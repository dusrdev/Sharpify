# CHANGELOG

## v2.3.0 - Unreleased (Pending Tests)

* Added `AtomicUpsert` overloads, for specific cases in which a user might want to get the value of some key, perform some operation on it, and then possibly update it. Doing this is the regular fashion as in reading the value, then checking before upserting (possibly) can introduce unwanted behavior as the value for said key may be changed by other threads while you try to process it, thereby meaning you process out-dated data. `AtomicUpsert` solves this, by blocking other threads from accessing the value for this specific key, while this operation is ongoing. Therefore ensuring no one can change it while you are processing it. All the main ways of reading data from `Database` have been restructured to ensure this with minimal overhead. **Important**: `Obsolete` reading methods (that don't begin with a "try") don't take these safety measures into account. If you still use these methods, I highly encourage you to use the other ones, as I remind you, In version 3.0.0, they will be flat out deleted.
* All JSON based `T` overloads now require a `JsonTypeInfo<T>` instead of the `JsonSerializerContext`, this change increases safety in cases where a `JsonSerializerContext` which didn't implement `T` would still be accepted and an exception would've been thrown at runtime, All the changes necessary at the client side are to add `.T` at the end of `JsonSerializerContext.Default` parameter.

## v2.2.0

**Possibly BREAKING** This version changes the base types of `Database` from `ReadOnlyMemory<byte>` to `byte[]`, apparently the change using `ReadOnlyMemory<byte>` produced invalid results as if the underlying array disappeared, which left users of a empty memory which has phantom meta-data.
To ensure this doesn't happen now `byte[]` is used to make sure all of the data remains, to prevent ownership issues, methods which return the actual `byte[]` values, now instead return a copy (albeit an efficient one), to make sure the data integrity in the database is safe. Same goes for inserts, where previously the source was placed in the database, this might have caused it to be prematurely garbage collected, thus leaving the database with a phantom metadata, now those actions create a copy and store it instead.

* As per the issues described above and their respective solutions, memory allocations should rise in some cases, however, it is a good trade-off for ensuring absolute integrity.
* Some users mentioned that as other databases, have options to query collections, ie, returning more than 1 to 1 item per key. It could greatly improve the usability of `Database` to have such a feature. In this version the feature is introduced.
`TryGetValue<T>` where `T : IMemoryPackable<T>`, now have `TryGetValues` overload, which returns a `T[]`, all under the same key. Respectively, an overload `UpsertMany<T>` was added with the same type restrictions. This also works from `IDatabaseFilter<T>`, these options should greatly improve useability when it comes to storing collections.
* Another possibly breaking change but one required for quality of life was to rename the `string value` overloads to `TryGetString` instead of `TryGetValue`, since they caused ambiguity which hindered the compilers ability to the infer the data type when using `var` which we all love.

## v2.1.0

* `DatabaseFilter{T}` type was changed from `readonly struct` to `class`, and it now implements the `IDatabaseFilter{T}` interface. the internal `CreateKey` that in the default implementation uses the type name and `:` to create a "filtered" key, is now marked as virtual. So that it is possible to inherit from `DatabaseFilter{T}` and override `CreateKey` to either use a different template, or even add to it, for example if you have nested generics, such as `TMemoryPackable<TOther>`, in which case the default `DatabaseFilter{T}` would not be able do distinguish between the inner generic, possibly causing issues with serialization and deserialization. The change to `class` also should be costly, as the database filter can be stored as a field as well, and used similarly to `dbContext` of other databases. In case overriding `CreateKey` is not enough, you can of course implement the whole `IDatabaseFilter{T}` interface if you so choose.

* Small **breaking** change in `DatabaseFilter{T}`, the filter will now use a `:` delimiter between the type name and the key, this means you keys won't be found if were upserted using pre-change filters. This is unfortunate but necessary change in order to 1. enable better filtering of keys from the `Database`, enabling searching and using split to get the second portion of the key. and 2. to lay the groundwork of possibly implementing more filters in the future.

* A temporary **FIX** to the **breaking** change in from the addition of the delimiter to `DatabaseFilter{T}` could be implemented rather easily using the new change from above, simply inherit from `DatabaseFilter{T}`, don't add anything, just override the `CreateKey` function to return `string.Concat(TName, key)`, this will behave exactly the same as the previous version of `DatabaseFilter{T}`. Nevertheless, I recommend this only as a temporary fix if you want to install the update but use existing data. at some point you should use the new feature.

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
