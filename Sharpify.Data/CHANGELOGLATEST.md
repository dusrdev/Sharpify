# CHANGELOG

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
