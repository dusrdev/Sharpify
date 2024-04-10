# CHANGELOG

## v1.8.0 - Unreleased

* Added `ToArrayFast(List)` and `ToListFast(Array)` extension methods, that efficiently create an array from list and the other way around.
* `SerializableObject<T>` and `MonitoredSerializableObject<T>` now require a `JsonTypeInfo<T>` instead of a `JsonSerializerContext` to improve type safety, as the `JsonSerializerContext` overloads cannot verify that the context has any implementation for the type, potentially leading to exceptions at runtime.
