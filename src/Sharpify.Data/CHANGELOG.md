# CHANGELOG

## v2.6.0

* Updated to support NET9
* Updated to use `Sharpify 2.5.0` and `MemoryPack 1.21.3`
* All `byte[]` value returning reads from the database, now return `ReadOnlyMemory<byte>` instead, previously, to maintain the integrity of the value, a copy was made and returned, because there wasn't any guarantee against modification, `ReadOnlyMemory<byte>` enforced this guarantee without creating a copy, if you just reading the data this is much more performant, and if you want to modify it, you can always create a copy at your own discretion.
* Decreased memory allocations for the `Func` based `Remove` method.
* Removed compiler directions that potentially could not allow the JIT compiler to perform Dynamic PGO.
* `Upsert{T}` overloads now have a `Func<T, bool> updateCondition` parameter that can be used to ensure that a condition is met before being updated, this is a feature of NoSQL databases that protects against concurrent writes overwriting each other. Now you can use this feature in `Sharpify.Data` as well.
  * Of course this feature is also available in `UpsertMany{T}` overloads, and also in the overloads of the `JsonTypeInfo T`.
  * To make it easier to see the result, these `Upsert` methods now return `bool`.
  * `False` will only be returned IF ALL of the following conditions are met:
    1. Previous value was stored under this key
    2. The previous value was successfully deserialized with the right type
    3. The `updateCondition` was not met
* `Database` now tracks changes (additions, updates, removals) and compares them serialization events, to avoid serialization if no updates occurred since the previous serialization.
  * This means that you can automate serialization without worrying about potential waste of resources, for example you could omit `SerializeOnUpdate` from the `DatabaseConfiguration`, then create a background task that serializes on a given interval for example with `Sharpify.Routines.Routine` or `Sharpify.Routines.AsyncRoutine`, and it will only actually serialize if updates occurred. This can significantly improve performance in cases where there are write peaks, but the database is mostly read from.
* You can now set the `Path` in the `DatabaseConfiguration` to an empty string `""` to receive an in-memory version of the database.
It still has serialization methods, but they don't perform any operations, they are essentially duds.
* `TryReadToRentedBuffer<T> where T : IMemoryPackable<T>` will now be able to retrieve the precise amount of needed space, so the size of the rented buffer will more accurately reflect the size of the data, this should help with dramatically improve performance when dealing with large objects. Before the buffer would've rented a capacity according to the length of the serialized object, meaning that the buffer was x times larger than needed when x is size(object) / size(byte). So the larger was each object, the `RentedBufferWriter` size would grow exponentially, now it grows linearly, maximizing efficiency.
* And minor optimizations (same as every other release 😜)

## v2.5.0

* Updated to use version 2.2.0 of `Sharpify` and later, and `MemoryPack` 1.21.1 and later.
* Removed apis that were previously marked as `Obsolete`
* An overload with `ReadOnlySpan{byte}` was added to `Upsert`, enables using `Upsert` on other types, such as lists or even pooled arrays. Be careful when using the `byte[]` overload, as now it doesn't create copies and instead inserts the reference instead to improve performance where needed. **DO NOT USE** this overload with buffers which you only temporarily own, for those use the new `ReadOnlySpan{byte}` overload.
* `UpsertMany<T>` now also has a `ReadOnlySpan{T}` accepting overload, it will not improve performance, but still adds flexibility, it doesn't replace the original `T[]` overload, it is an alternative. If the main `T[]` suits your context, as in you already have a fixed size array, it will actually be more performant.
* New method `Database.TryReadToRentedBuffer` now rents an appropriately sized `RentedBufferWriter{T}` and attempts to write the value to it, then return it. If it is successful, the result can be viewed with `RentedBufferWriter{T}.WrittenSpan` and other apis, if not successful (i.e key not found), a disabled `RentedBufferWriter{T}` will be returned, it can be checked with the `RentedBufferWriter{T}.IsDisabled` property.
  * There is also an optional parameter for `reservedCapacity`, this will make sure the buffer has a matching amount free capacity after writing the data, an explanation of why this is useful will be lower down the page.
  * There are overloads in `Database` for both `byte` and `T : IMemoryPackable{T}`, as well as methods in `MemoryPackableDatabaseFilter{T}` and `FlexibleDatabaseFilter{T}`.
* **POSSIBLY BREAKING** for those who use the `JSON` serialized `T` overloads, in the previous versions, the `JsonSerializer` was used to generate a `string`, which then passed to `MemoryPack` for secondary serialization. To improve performance, now `JsonSerializer` directly serializes to `byte[]` which means using these types is now both faster and more memory efficient than before. But if you try to read values with the new version which were serialized in the old version, there may be inconsistencies which may cause errors.
  * If you encounter such issues, you may want to synchronize manually as follows:
  * Read the values with `TryGetString`, then use `JsonSerializer.Deserialize` on the strings to get the actual values, you can then proceed to upsert those values with the `JSON` overloads on the same keys.
* Some of the new changes were also leveraged internally to improve performance/memory allocations in various places.

## v2.4.1

* Updated to version 2.0.0 of `Sharpify`.

If you use an older version of `Sharpify` this update is not a requirement, it mainly addresses a fix since `DecryptBytes` of `AesProvider` in `Sharpify` now has 2 overloads with 2 parameters, and the compiler seems to trim the wrong one, unless the optional parameter is specified.

* Also `preFilter` in `Database.Remove()` was renamed to `keyPrefix` to better signify its purpose. this change doesn't alter behavior.

## v2.4.0

* Added an overload for `Remove` which takes in a `Func<string, bool> keySelector`, this function is more optimized then using if you were to iterate yourself and call the old `Remove` as this one will execute serialization only once at the end, and only if removals actually happened (selector actually matched at least one key).
  * The new `Remove` method also has an overload that accepts a `string? preFilter` as well, which can be used to only check keys that start with `preFilter` (the `keySelector` doesn't need to account for it, it will applied to a slice if the `preFilter` is matched), if left `null` it will be ignored.
  * This addition was also propagated to both implementations of the `IDatabaseFilter<T>`, i.e `MemoryPackDatabaseFilter<T>` and `FlexibleDatabaseFilter<T>`, both of which modify the incoming delegate to the use the filtered key, enabling simple delegate matches without relying on implementation details, they don't have the option to use a `preFilter` as they themselves use the statically generated type filters they create.
* To accommodate the `Remove` methods, `MemoryPackDatabaseFilter<T>` and `FlexibleDatabaseFilter<T>` now create a `public static readonly string KeyFilter` property, which is the prefix they append to the keys, this is used internally for `Remove` but perhaps the could help if you need to inherit from these classes and override the `Remove` method.
  * Both of them also use `KeyFilter` internally to generate the filtered keys in a slightly more efficient way to before.
  * The `static readonly` field that contained the generic type name was also removed as it was integrated into `KeyFilter` at with no additional cost.

## v2.3.0

* The codebase was refactored and separated into smaller files, to make it much easier to work with.
* `Upserts` of all overloads and entry points will now throw an exception if the `value` is `null`. This change was made to ensure the integrity of `TryGetValue` (from all variants) as it checks nullability of the value to ensure the key exists. This is also no point to add null values, as they are not meaningful data, by enforcing not null, the code becomes simpler, and less error prone.
* Added `StringEncoding` choice to `DatabaseConfiguration`, it defaults to `UTF8`, but can also be `UTF16`, `UTF8` requires less memory in default cases, but `UTF16` can be more efficient if most of the strings are `Unicode`.
* The factory methods named `Create` and `CreateAsync` were renamed to `CreateOrLoad` and `CreateOrLoadAsync` respectively, which better explains exactly what they do at a glance. This should make more sense to code reviewers who are not familiar with the package.
* **Filtering**
  * `IDatabaseFilter` which is the abstraction of the filters now has proxies for `Serialize` and `SerializeAsync` which previously couldn't be accessed via this layer, but may be required if `SerializeOnUpdate=false`.
  * `DatabaseFilter<T> where T : IMemoryPackable<T>` was renamed to `MemoryPackDatabaseFilter<T>`, and the Database method to create an instance was renamed from `Database.FilterByType<T>` to `CreateMemoryPackFilter<T>`.
  * A new filter is introduced: `FlexibleDatabaseFilter<T> where T : IFilterable<T>`, which enables filtering on any type, without depending on `MemoryPack` implementation, for this an interface `IFilterable<T>` was also added, the interface will require implementing a few methods which dictate how to serialize and deserialize the specific value type. The `FlexibleDatabaseFilter` inturn will use those implementation to provide the same experience. The filter can be created by `Database.CreateFlexibleFilter<T>`
* All JSON based `T` overloads now require a `JsonTypeInfo<T>` instead of the `JsonSerializerContext`, this change increases safety in cases where a `JsonSerializerContext` which didn't implement `T` would still be accepted and an exception would've been thrown at runtime, All the changes necessary at the client side are to add `.T` at the end of `JsonSerializerContext.Default` parameter.

### Workaround for broken NativeAot support from MemoryPack

As of writing this, MemoryPack's NativeAot support is broken, for any type that isn't already in their cached types, the `MemoryPackFormatterProvider` uses reflection to get the formatter, which fails in NativeAot.
As a workaround, we need to add the formatters ourselves, to do this, take any 1 static entry point, that activates before the database is loaded, and add this:

```csharp
// for every T type that relies on MemoryPack for serialization, and their inheritance hierarchy
// This includes types that implement IMemoryPackable (i.e types that are decorated with MemoryPackable)
MemoryPackFormatterProvider.Register<T>();
// If the type is a collection or dictionary use the other corresponding overloads:
MemoryPackFormatterProvider.RegisterCollection<TCollection, TElement>();
// or
MemoryPackFormatterProvider.RegisterDictionary<TDictionary, TKey, TValue>();
// and so on...
// for all overloads check peek the definition of MemoryPackFormatterProvider, or their Github Repo
```

**Note:** Make sure you don't create a new static constructor in those types, `MemoryPack` already creates those, you will need to find a different entry point.

With this the serializer should be able to bypass the part using reflection, and thus work even on NativeAot.

P.S. The base type of the Database is already registered the same way on its own static constructor.

### Announcement

Internal benchmarks already show a considerable performance improvement in the .NET8 version vs .NET7, and there are already multiple cases where the separate implementations have to be made in order to change to accommodate both versions, with .NET9 release approaching, more cases like this are expected, due the added complexity, .NET7 support will be dropped with the release of version 2.4.0 in the future. Codebases that are enable to migrate to newer .NET version will be forced to use older version of Sharpify.Data.

.NET8 support will be maintained much longer since it is an LTS release.

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
