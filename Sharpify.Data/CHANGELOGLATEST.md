# CHANGELOG

## v2.4.1

* Updated to version 2.0.0 of `Sharpify`.

If you use an older version of `Sharpify` this update is not a requirement, it mainly addresses a fix since `DecryptBytes` of `AesProvider` in `Sharpify` now has 2 overloads with 2 parameters, and the compiler seems to trim the wrong one, unless the optional parameter is specified.

* Also `preFilter` in `Database.Remove()` was renamed to `keyPrefix` to better signify its purpose. this change doesn't alter behavior.

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
