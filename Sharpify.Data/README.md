# Sharpify.Data

An extension of `Sharpify` focused on data.

## Features

* `Database` is key-value-pair datastore of `string`->`ReadOnlyMemory<byte>` with converters that is optimized for concurrency, memory efficiency and performance, and enables 2-layer encryption, both per the database as a whole and per key.
* The most important converter is using types which implement `IMemoryPackable<T>` which is any type that was decorated with the `MemoryPackable` attribute from [MemoryPack](https://github.com/Cysharp/MemoryPack)
* Working with such types allows usage of `DatabaseFilter{T}` enabling the single database object (and file) to store and filter the data by the type allowing use cases similar to different `table` types in more common databases.

## Notes

* Initialization of both types is with regular and async factory methods from each class, they will guide you for using the options of each respective class.
* The heart of the performance of these databases which use [MemoryPack](https://github.com/Cysharp/MemoryPack) for extreme performance binary serialization.
* `Database` has upsert overloads which support any `IMemoryPackable` from [MemoryPack](https://github.com/Cysharp/MemoryPack).
* Both `Database` implements `IDisposable` and should be disposed after usage to make sure all resources are released, this should also prevent possible issues if the object is removed from memory while an operation is ongoing (i.e the user closes the application when a write isn't finished)

## Contact

For bug reports, feature requests or offers of support/sponsorship contact <dusrdev@gmail.com>
