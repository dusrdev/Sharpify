# CHANGELOG

## v2.4.0

* Added an overload for `Remove` which takes in a `Func<string, bool> keySelector`, this function is more optimized then using if you were to iterate yourself and call the old `Remove` as this one will execute serialization only once at the end, and only if removals actually happened (selector actually matched at least one key).
  * The new `Remove` method also has an overload that accepts a `string? preFilter` as well, which can be used to only check keys that start with `preFilter` (the `keySelector` doesn't need to account for it, it will applied to a slice if the `preFilter` is matched), if left `null` it will be ignored.
  * This addition was also propagated to both implementations of the `IDatabaseFilter<T>`, i.e `MemoryPackDatabaseFilter<T>` and `FlexibleDatabaseFilter<T>`, both of which modify the incoming delegate to the use the filtered key, enabling simple delegate matches without relying on implementation details, they don't have the option to use a `preFilter` as they themselves use the statically generated type filters they create.
* To accommodate the `Remove` methods, `MemoryPackDatabaseFilter<T>` and `FlexibleDatabaseFilter<T>` now create a `public static readonly string KeyFilter` property, which is the prefix they append to the keys, this is used internally for `Remove` but perhaps the could help if you need to inherit from these classes and override the `Remove` method.
  * Both of them also use `KeyFilter` internally to generate the filtered keys in a slightly more efficient way to before.
  * The `static readonly` field that contained the generic type name was also removed as it was integrated into `KeyFilter` at with no additional cost.

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
