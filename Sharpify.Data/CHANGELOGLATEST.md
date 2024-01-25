# CHANGELOG

## v2.1.0

* Small **breaking** change in `DatabaseFilter{T}`, the filter will now use a `:` delimiter between the type name and the key, this means you keys won't be found if were upserted using pre-change filters. This is unfortunate but necessary change in order to 1. enable better filtering of keys from the `Database`, enabling searching and using split to get the second portion of the key. and 2. to lay the groundwork of possibly implementing more filters in the future.

* `DatabaseFilter{T}` type was changed from `readonly struct` to `class`, and it now implements the `IDatabaseFilter{T}` interface. the internal `CreateKey` that in the default implementation uses the type name and `:` to create a "filtered" key, is now marked as virtual. So that it is possible to inherit from `DatabaseFilter{T}` and override `CreateKey` to either use a different template, or even add to it, for example if you have nested generics, such as `TMemoryPackable<TOther>`, in which case the default `DatabaseFilter{T}` would not be able do distinguish between the inner generic, possibly causing issues with serialization and deserialization. The change to `class` also should be costly, as the database filter can be stored as a field as well, and used similarly to `dbContext` of other databases. In case overriding `CreateKey` is not enough, you can of course implement the whole `IDatabaseFilter{T}` interface if you so choose.

`DatabaseFilter{T}` was initially designed better in the context of APIs, no longer offering the `Get`, but instead using `TryGetValue`, while it might require 1 line of code more, when reading and writing the code, it is less ambiguous, before, a null or default result could indicate `not found`, `failed to deserialized`, and even upserted as null. Now if `TryGetValue` returns false there can only be one reason and that is that the key did not exist.

* To improve this conciseness, `Database` now has `TryGetValue` offerings, for regular `ReadOnlyMemory{byte}` output, `IMemoryPackable{T}` and `string`. These are now the preferred APIs to use when retrieving values.
* The old `Get` variants of `Database` are now marked as `Deprecated` to signal they shouldn't be used. This was made to reduce the amount of breaking changes in this version, the `Get` variants will stay on as `Deprecated` until the next `Major` version, at which point they will be deleted. I hope this gives you enough time to "migrate".

* `UpsertAsString` and `UpsertAsT`(JSON version), are now also named just `Upsert`, their overload is inferred from the type of the arguments as string is not `IMemoryPackable{T}` and the JSON `T` version requires a `JsonSerializerContext`.
* Also added `TryGetValue` overloads for JSON `T`, you will know them because they both require a `JsonSerializerContext`. apparently before this version you could only `Upsert` a JSON `T`, I apologize for the oversight.
