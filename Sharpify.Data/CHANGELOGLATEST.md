# CHANGELOG

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
