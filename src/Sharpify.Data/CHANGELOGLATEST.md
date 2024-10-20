# CHANGELOG

## v2.6.0 - Alpha

* All `byte[]` value returning reads from the database, now return `ReadOnlyMemory<byte>` instead, previously, to maintain the integrity of the value, a copy was made and returned, because there wasn't any guarantee against modification, `ReadOnlyMemory<byte>` enforced this guarantee without creating a copy, if you just reading the data this is much more performant, and if you want to modify it, you can always create a copy at your own discretion.
* Decreased memory allocations for the `Func` based `Remove` method.
* Removed compiler directions that potentially could not allow the JIT compiler to perform Dynamic and regular PGO.
* Added accessability modifiers to `ReadOnlySpan` based parameters.
* `Upsert{T}` overloads now have a `Func<T, bool> updateCondition` parameter that can be used to ensure that the previously stored value passes a condition before being updated, this is a feature of NoSQL databases that protects against concurrent writes overwriting each other. Now you can use this feature in `Sharpify.Data` as well.
  * Of course this feature is also available in `UpsertMany{T}` overloads, and also in the overloads of the `JsonTypeInfo T`.
  * To make it easier to see the result, these `Upsert` methods now return `bool`.
  * `False` will only be returned if all of the next conditions are met:
    1. Previous value was stored under this key
    2. The previous value was successfully deserialized
    3. The `updateCondition` was not met
* `Database` now tracks changes (additions, updates, removals) and compares them serialization events, to avoid serialization if no updates occurred since the previous serialization.
  * This means that you can automate serialization without worrying about potential waste of resources, for example you could omit `SerializeOnUpdate` from the `DatabaseConfiguration`, then create a background task that serializes on a given interval for example with `Sharpify.Routines.Routine` or `Sharpify.Routines.AsyncRoutine`, and it will only actually serialize if updates occurred. This can significantly improve performance in cases where there are write peaks, but the database is mostly read from.
* You can now set the `Path` in the `DatabaseConfiguration` to an empty string `""` to receive an in-memory version of the database.
It still has serialization methods, they don't perform any operations, they are essentially duds.
* `TryReadToRentedBuffer<T> where T : IMemoryPackable<T>` will now be able to retrieve the price amount of needed space, so the size of the rented buffer will more accurately reflect the size of the data, this should help with performance drastically with large objects. Before the buffer would've rented a capacity according to the length of the serialized object, meaning that the buffer was x times larger than needed where x is size(object) / size(byte). So the larger the object here, the more noticeable the memory efficiency after the change.

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
