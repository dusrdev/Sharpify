# Sharpify.Data

An extension of `Sharpify` focused on data.

## Features

* `Database` is key-value-pair datastore of `string`->`ReadOnlyMemory<byte>` with converters that is optimized for concurrency, memory efficiency and performance, and enables 2-layer encryption, both per the database as a whole and per key.
* The most important converter is using types which implement `IMemoryPackable<T>` which is any type that was decorated with the `MemoryPackable` attribute from [MemoryPack](https://github.com/Cysharp/MemoryPack)
* Working with such types allows usage of `DatabaseFilter{T}` enabling the single database object (and file) to store and filter the data by the type allowing use cases similar to different `table` types in more common databases.

## Notes

* Initialization of with regular and async factory methods, they will guide you for using the options of configuration.
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

The data is generated using [Bogus](https://github.com/bchavez/Bogus), and as you can see, all of the properties are strings and of decent length, so that the benchmark wouldn't lie by using "lightweight" objects.

<img width="1000" alt="SCR-20240121-lnrr" src="https://github.com/dusrdev/Sharpify/assets/8972626/20ac0812-1588-45a8-bedb-d8dc028f3c01">

The database is very fast, while serialization scales rather linearly (No way around it), Upserts, and Retreive are constant time operations.

Some clarification over the stats: Memory Allocation (right most column), in this case shows not allocation "per operation" but rather "allocation per scale" of operation, as everything essentially uses array pooling, the column shows the memory that the array was required to allocate, but once it is allocated, every subsequent operation that requires the same scale of memory, doesn't need to allocate anything at all. You can also see that for `Serialize` which is obviously the biggest and most resource heavy operation of the database, `Gen 0` is none, so there is no need for garabage collection at all.
While the other operations show some `Gen 0` althought in very small amounts, this is not buffer related at all, rather just allocation of a light buffer management class.

## Contact

For bug reports, feature requests or offers of support/sponsorship contact <dusrdev@gmail.com>
