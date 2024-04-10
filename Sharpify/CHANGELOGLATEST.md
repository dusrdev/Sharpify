# CHANGELOG

## v1.8.0 - Unreleased

* Added `ToArrayFast(List)` and `ToListFast(Array)` extension methods, that efficiently create an array from list and the other way around.
* `SerializableObject<T>` and `MonitoredSerializableObject<T>` now require a `JsonTypeInfo<T>` instead of a `JsonSerializerContext` to provide more type safety, as the `JsonSerializerContext` overloads cannot verify that the context has any implementation for the type, potentially leading to exceptions at runtime.
  * Also, An `Action<T>` overload was added to `Modify` to reduce reallocations when `T : class`, for structs you should use the older `Modify` method with the `Func` as the reference of the struct won't be modified. An appropriate message was added for guidance in the method summaries.
