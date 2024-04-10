# CHANGELOG

## v1.8.0 - Unreleased

* Added `WrittenRange` computed property to `StringBuffer` and `AllocatedStringBuffer` that will return the used range.
* Added a `FormatBytesNonAllocated` alternative to `FormatBytes`, it doesn't allocate the string at all and reuses an internal buffer. If you only use it for printing for example, and a `ReadOnlySpan<char>` overload for printing can be used, the performance gains by using this method could be immense.
* Added `NonAllocated` overloads to `Format(TimeSpan)`, `ToRemainingDuration(TimeSpan)` and `ToTimeStamp(TimeSpan)`.
* Added `ToArrayFast(List)` and `ToListFast(Array)` extension methods, that efficiently create an array from list and the other way around.
* `SerializableObject<T>` and `MonitoredSerializableObject<T>` now require a `JsonTypeInfo<T>` instead of a `JsonSerializerContext` to improve type safety, as the `JsonSerializerContext` overloads cannot verify that the context has any implementation for the type, potentially leading to exceptions at runtime.
