# CHANGELOG

## v2.5.0

* Updated to support .NET 9.0 and optimized certain methods to use .NET 9 specific API's wherever possible.
* Added `BufferWrapper<T>` which can be used to append items to a `Span<T>` without managing indexes and capacity. This buffer also implement `IBufferWriter<T>`, and as a `ref struct implementing an interface` it is only available on .NET 9.0 and above.
* `Utils.String.FormatBytes` now uses a much larger buffer size of 512 chars by default, to handle the edge case of `double.MaxValue` which would previously cause an `ArgumentOutOfRangeException` to be thrown or similarly any number of bytes that would be bigger than 1024 petabytes. The result will now also include thousands separators to improve readability.
  * The inner implementation that uses this buffer size is pooled so this should not have any impact on performance.
