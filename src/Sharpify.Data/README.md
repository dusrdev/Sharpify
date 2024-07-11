# Sharpify.Data

An extension of `Sharpify` focused on data.

## Features

* `Database` is key-value-pair datastore of `string`->`byte[]` with converters that is optimized for concurrency, memory efficiency and performance, and enables 2-layer encryption, both per the database as a whole and per key.
* The most important converter is using types which implement `IMemoryPackable<T>` which is any type that was decorated with the `MemoryPackable` attribute from [MemoryPack](https://github.com/Cysharp/MemoryPack)
* Working with such types allows usage of `DatabaseFilter{T}` enabling the single database object (and file) to store and filter the data by the type allowing use cases similar to `table` types in more common databases.
* `NativeAot` supported -> see [Guide](#nativeaot-guide)

## Notes

* Initialization of with regular and async factory methods, they will guide you for using the options of configuration.
* It is crucial to use the factory methods for database initialization, and **NOT** use activators or constructors, the factory methods select configuration specific abstractions that are optimized per the the type of database you want.
* The heart of the performance of these databases which use [MemoryPack](https://github.com/Cysharp/MemoryPack) for extreme performance binary serialization.
* `Database` has upsert overloads which support any `IMemoryPackable` from [MemoryPack](https://github.com/Cysharp/MemoryPack).
* Both `Database` implements `IDisposable` and should be disposed after usage to make sure all resources are released, this should also prevent possible issues if the object is removed from memory while an operation is ongoing (i.e the user closes the application when a write isn't finished)
* The database is key-value-pair based, and operation on each key have O(1) complexity, serialization scales rather linearly (No way around it).
* For very large datasets, there might be more suitable databases, but if you still want to use this, you could enable `[gcAllowVeryLargeObjects](https://learn.microsoft.com/en-us/dotnet/framework/configure-apps/file-schema/runtime/gcallowverylargeobjects-element), as per the Microsoft docs, on 64 bit system it should allow the object to be larger than 2GB, which is normally the limit.
* To ensure integrity data copies are kept to a minimum, and allocations are designed to happen only when required to ensure data integrity (i.e to ensure the database stores real data, and to ensure the actual data is not exposed to the outside), the database uses pooling for any disposable memory operations to ensure minimal GC overhead.

## NativeAot Guide

As of writing this, `MemoryPack`'s NativeAot support is broken, for any type that isn't already in their cached types, the `MemoryPackFormatterProvider` uses reflection to get the formatter (that includes types decorated with `MemoryPackable` which in turn implement `IMemoryPackable<T>`), which fails in NativeAot.
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

## Contact

For bug reports, feature requests or offers of support/sponsorship contact <dusrdev@gmail.com>
