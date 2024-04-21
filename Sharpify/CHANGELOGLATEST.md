# CHANGELOG

## v1.8.0

* `AllocatedStringBuffer` now has variation that accepts a `char[]` as input buffer, this version which also has a corresponding overload in `StringBuffer.Create` supports an implicit converter to `ReadOnlyMemory<char>`, intellisense will allow you to use this converter even you used `Span<char>` as a buffer, but doing so will cause an exception to be thrown, as a `Span<char>` can be `stack allocated` and won't be able to be referenced.
* The method above paved the way to creating a `FormatNonAllocated(TimeSpan)`, `ToRemainingDurationNonAllocated(TimeSpan)`, `ToTimeStampNonAllocated(TimeSpan)`, and `FormatBytesNonAllocated(double)`, these overloads would format directly to span, and return a slice, this would completely bypass the `string` allocation, which would've otherwise caused large amounts of GC overhead in frequent calls.
* Added `ToArrayFast(List)` and `ToListFast(Array)` extension methods, that efficiently create an array from list and the other way around.
* `SerializableObject<T>` and `MonitoredSerializableObject<T>` now require a `JsonTypeInfo<T>` instead of a `JsonSerializerContext` to improve type safety, as the `JsonSerializerContext` overloads cannot verify that the context has any implementation for the type, potentially leading to exceptions at runtime.
