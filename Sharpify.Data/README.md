# Sharpify.Data

An extension of `Sharpify` focused on data.

## Features

* `Database` is key-value-pair datastore of `string`->`byte[]` with converters that is optimized for concurrency, memory efficiency and performance, and enables 2-layer encryption, both per the database as a whole and per key.
* The most important converter is using types which implement `IMemoryPackable<T>` which is any type that was decorated with the `MemoryPackable` attribute from [MemoryPack](https://github.com/Cysharp/MemoryPack)
* Working with such types allows usage of `DatabaseFilter{T}` enabling the single database object (and file) to store and filter the data by the type allowing use cases similar to `table` types in more common databases.

## Notes

* Initialization of with regular and async factory methods, they will guide you for using the options of configuration.
* It is crucial to use the factory methods for database initialization, and **NOT** use activators or constructors, the factory methods select configuration specific abstractions that are optimized per the the type of database you want.
* The heart of the performance of these databases which use [MemoryPack](https://github.com/Cysharp/MemoryPack) for extreme performance binary serialization.
* `Database` has upsert overloads which support any `IMemoryPackable` from [MemoryPack](https://github.com/Cysharp/MemoryPack).
* Both `Database` implements `IDisposable` and should be disposed after usage to make sure all resources are released, this should also prevent possible issues if the object is removed from memory while an operation is ongoing (i.e the user closes the application when a write isn't finished)

## Sample Benchmarks

The benchmarks are of `Person` record, which is implement as such:

```csharp
[MemoryPackable]
public partial record Person {
  public string Name { get; set; } = "";
  public string Email { get; set; } = "";
  public string Username { get; set; } = "";
  public string Password { get; set; } = "";
  public string Phone { get; set; } = "";
  public string Website { get; set; } = "";
}
```

The database is very fast and efficient, while serialization scales rather linearly (No way around it), `Upserts`, and `Retrievals` are constant time operations.

To ensure integrity data copies are kept to a minimum, and allocations are designed to happen only if it is to ensure data integrity (i.e to ensure the database stores real data, and to ensure the actual data is not exposed to the outside), the database uses pooling for any disposable memory operations to ensure minimal GC overhead and memory allocation.

## Contact

For bug reports, feature requests or offers of support/sponsorship contact <dusrdev@gmail.com>
