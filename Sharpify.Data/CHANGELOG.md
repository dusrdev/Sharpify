# CHANGELOG

## v1.2.0

### BREAKING

* Values that were upserted to `Database` as `string`, and possibly `T` when using the `JSON` serialization overloads, can fail to retrieved as the internal `string` to `byte[]` serializer was changed.
* `Database` value type was changed from `byte[]` to `ReadOnlyMemory<byte>` to have smaller footprint and use high performance apis with less conversions.

It is recommended to either run some tests to ensure the changes don't break existing code, or perform adjustments before upgrading.

### WHAT IS NEW

* `DatabaseFilter{T}` is a `readonly struct` filter of the `Database` based on a types that are `IMemoryPackable`, the filter should provide an AOT-friendly way to use multiple types in the same file, while ensuring no unforeseen `deserialization` issues occur because of key exists but with value of different type. `DatabaseFilter{T}` is simple and AOT-friendly because it does no changes to `Database` whatsoever, instead just wraps the key that it uses with a modification that includes the name of the type. It is only an abstraction. Nevertheless, this abstraction is very powerful as it takes no additional effort from the user, and allowing the user to create generic consumer classes, all of which use the same database, but injecting the filter instead of the database, making it virtually impossible that a generic class will try to use a value of a different type.

## v1.1.0

* **BREAKING** `UpsertAsT` functions signature changed to force usage of new parameter, which is a `JsonSerializerContext` that can serialize `T`.
* Simplified structures of inner data representation, slightly reducing assembly size.
* `Sharpify.Data` is now fully AOT compatible

## v1.0.3

* Upgraded concurrency synchronization model of `Database` and `Database{T}` to get more accurate reads when other threads are writing.
* Both now implement `IDisposable` to release the resources of the synchronization and should be disposed of properly, but since they are designed to be used throughout the lifetime of the application, it isn't absolutely crucial to do this, and the implement finalizer should take care of this, if disposing of it from your end is inconvenient

## v1.0.2

* Fixed issue where an exception would be thrown if `Upsert` overrides a key. it should by design override.

## v1.0.1

* Further optimized both database, heavily utilizing array pooling for encryption

## v1.0.0

* Initial release - check github for information
