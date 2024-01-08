# Sharpify.Data

An extension of `Sharpify` focused on data.

## Features

* `Database` is key-value-pair datastore of `string`->`byte[]` with converters that is optimized for concurrency, memory efficiency and performance, and enables 2-layer encryption, both per the database as a whole and per key.
* `Database{T}` is a key-value-pair datastore of `string`->`T` that requires conversion delegates `T`->`byte[]` and back, that enables natively storing the `T`s in memory, which makes reads even faster, but at the cost of per key encryption.

## Notes

* Initialization of both types is with regular and async factory methods from each class, they will guide you for using the options of each respective class.
* Although `Database{T}` reads are faster than `Database`, they are still both lightning fast, as most operations are O(1) on a native dictionary.
* `byte[]` is the heart of the performance of these databases which use [MemoryPack](https://github.com/Cysharp/MemoryPack) for extreme performance binary serialization.
* `Database` has upsert overloads which support any `IMemoryPackable` from [MemoryPack](https://github.com/Cysharp/MemoryPack), that enables to match the performance of `Database{T}` with less abstraction.
* `Database` retrieval or upserts of values without per-key encryption is equal or rivals the performance of `Database{T}`,
When used together with `IMemoryPackable` it will make a persistent-in-memory cache that will rival any other.
* Both `Database` and `Database{T}` implement `IDisposable` and should be disposed after usage to make sure all resources are released, this should also prevent possible issues if the object is removed from memory while an operation is ongoing (i.e the user closes the application when a write isn't finished)

## Contact

For bug reports, feature requests or offers of support/sponsorship contact <dusrdev@gmail.com>
