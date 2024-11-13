# Sharpify.Data

An extension of `Sharpify` focused on data.

## Features

* `Database` is the base type for the data base, it is key-value-pair based local database - saved on disk.
* `IDatabaseFilter<T>` is an interface which acts as an alternative to `DbContext` and provides enhanced type safety for contexts.
* `MemoryPackDatabaseFilter<T>` is an implementation which focuses on types that implement `IMemoryPackable<T>` from `MemoryPack`.
* `FlexibleDatabaseFilter<T>` is an implementation focusing on types which need custom serialization logic. To use this, you type `T` will need to implement `IFilterable<T>` which has methods for serialization and deserialization of single `T` and `T[]`. If you can choose to implement only one of the two.
* **Concurrency** - `Database` uses highly performant synchronous concurrency models and is completely thread-safe.
* **Disk Usage** - `Database` tracks inner changes and skips serialization if no changes occurred, enabling usage of periodic serialization without resource waste.
* **GC Optimization** - `Database` heavily uses pooling for encryption, decryption, type conversion, serialization and deserialization to minimize GC overhead, very rarely does it allocate single-use memory and only when absolutely necessary.
* **HotPath APIs** - `Database` is optimized for hot paths, as such it provides a number of APIs that specifically combine features for maximum performance and minimal GC overhead. Like the `TryReadToRentedBuffer<T>` methods which is optimized for adding data to a table.
* **Runtime Optimization** - Upon initialization, `Database` chooses specific serializers and deserializers tailored for specific configurations, minimizing the amount of runnable code during runtime that would've been wasted on different checks.

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
