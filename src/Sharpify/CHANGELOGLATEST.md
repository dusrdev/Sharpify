# CHANGELOG

## v2.3.0

This release contains multiple **BREAKING CHANGES**, please read the following list carefully to see if and where you need to make changes to your code.

* `AllocatedStringBuffer` now longer contains a reference to the original array as it defeated its main purpose of being a purely stack based abstraction, you can still use on a pre-allocated array but you would have to maintain the reference yourself. If you don't want to allocate an array, use the `StringBuffer` which rents one from the pool.
  * The removal of the array, now means that the method which returned a `ReadOnlyMemory<char>` from the buffer was also removed, as it is no longer possible to ensure that the buffer even existed on the heap (it could have been stack allocated), disallowing the use of `ReadOnlyMemory<char>`.
* Both `AllocatedStringBuffer` and `StringBuffer` contain new things:
  * `this[int]` is now an indexer which returns the character at a given index. As with `this[Range]` be careful with the bounds as it will throw an exception if you go out of bounds.
  * `WrittenSpan` is a computed property which returns a `ReadOnlySpan<char>` from the start of the buffer to the current position.
  * `Length` is now a `public readonly` field which returns the length of the buffer. This could help users debug or understand the buffers better.
* `Utils.FormatBytesNonAllocated` was removed as the api was confusing and there are no better ways to handle all of its use cases:
  * `FormatBytes` which previously used it, now uses stack space to format the bytes.
  * `FormatBytesInRentedBuffer` is a replacement which rents a buffer for you with the help of `StringBuffer`, writes the data to it, and returns the buffer, you can view the internals with `StringBuffer.WrittenSpan` which can prevent any string allocation if unnecessary. You either use a `using statement` or call `.Dispose()` on the return buffer to make sure the pooled array is returned after use.
* Same changes were also made to:
  * elapsed time: `TimeSpan.Format` and now `TimeSpan.FormatInRentedBuffer`
  * remaining duration: `TimeSpan.ToRemainingDuration` and now `TimeSpan.ToRemainingDurationInRentedBuffer`
  * time stamps: `DateTime.ToTimeStamp` and now `DateTime.ToTimeStampInRentedBuffer`
* `RentedBufferWriter{T}` now has a static factory method `Create`, it does exactly the same as the constructor and was mainly added for users who prefer those.
* `RentedBufferWriter{T}` now has a computed property `CurrentPosition` which returns the current position in the buffer. Which means `GetMemorySlice` and `GetSpanSlice` are now usable in more scenarios.
* `AesProvider` now uses internally uses `RentedBufferWriter` which allows should increase stability.
* `LazyLocalPersistentDictionary` now also uses `RentedBufferWriter`.
