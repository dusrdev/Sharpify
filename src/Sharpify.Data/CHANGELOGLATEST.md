# CHANGELOG

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

### New APIs: Purposely Built For Performance

Upserting a `ReadOnlySpan` instead of `[]`, or reading data into a `RentedBufferWriter` may seem unnecessary by themselves, but using both together can unlock the next level of performance in some scenarios.

Consider the following example:

you have a server which manages some items of different categories, and you use a collection to hold items of type `book` for example, and a request comes in to add a new book.

#### Before this change

1. Retrieve allocated `Book[]`
2. Allocate a new `List<Book>`
3. Copy said `Book[]` to `List<Book>`
4. Add new book at the end of said `List<Book>`
5. Copy `List<Book>` to a new `Book[]`
6. Upsert said `Book[]`

This means for `N` books, we allocate `3N` items, this is a big waste...

### After this change

1. Retrieve `RentedBufferWriter{T}` using `TryReadToRentedBuffer` with `reservedCapacity=1` to add 1 `Book`
2. Insert new `Book` with `RentedBufferWriter.WriteAndAdvance(Book)`
3. Upsert with `RentedBufferWriter.WrittenSpan`

Firstly this is so much simpler, secondly, the performance gains are huge, `RentedBufferWriter` as per the name uses array pooling, in this scenario you don't need the actual array, so this is the perfect place for them, so we write to a pooled array, which has enough reserved capacity, which we use to add the book, upsert the written portion, and after this usage `RentedBufferWriter` which implements `IDisposable` automatically returns the array to the array pool.

### Reminder: Workaround for broken NativeAot support from MemoryPack

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
