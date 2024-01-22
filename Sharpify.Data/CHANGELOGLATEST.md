# CHANGELOG

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
