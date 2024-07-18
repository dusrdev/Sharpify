# CHANGELOG

## v2.1.0

* Changes to `RentedBufferWriter{T}`:
  * `RenterBufferWriter{T}` no longer throws an exception when the initial capacity is set to 0, instead it will just be disabled, this can be checked with the `.IsDisabled` property. Setting the initial capacity to a negative number will still throw an exception. This change was made to accommodate use cases where a `RentedBufferWriter` could be used as a return type, before an "invalid" operation, could not be attained, as it would've been required to give a valid capacity in any case, lest you risk a runtime exception. Now you could actually return a "Disabled" `RentedBufferWriter` if you set the capacity to 0, which intuitively means that the buffer doesn't actually have a backing array, and all operations would throw an exception.
  * To increase it's usability, a method `WriteAndAdvance` was also added that accepts either a single `T` or a `ReadOnlySpan{T}`, it checks if there is enough capacity to write the data to the buffer, if so it writes it and advances the position automatically.
  * A secondary access `ref T[] GetReferenceUnsafe` was added, to allow passing the inner buffer to methods that write to a `ref T[]` which previously required using unsafe code to manipulate the pointers. As implied by the name, this uses `Unsafe` to acquire the reference, and should only be used if you are carful and know what you are doing.
* Collection extension methods such as `ToArrayFast()` and `ToListFast()` were made deprecated, use the `ToArray()` and `ToList()` LINQ methods instead, with time they become the fastest possible implementation and will continue to improve, the performance gap is already minimal, and only improves speed, not memory allocations, which makes it negligible.

### Deprecation

A lot of features of the package are already deprecated, and some will continue in this direction as Microsoft seems to really focus on improving performance in the latest versions of C#, some methods that this package originally offered to provide a better performance than what is already in core language, will instead provide worse performance, at which point they've served their purpose.

As such, in the next major versions, many of these features will be removed. To avoid breaking old codebases, for example for `.NET 7` users, the best way forward would be to stop supporting this `.NET` version in the future versions of `Sharpify`, this means that existing users will still have what they used, but the users of the newer technologies will be able to use newer versions of `Sharpify` that rely on said technologies to remain dynamic, and stay ahead on the performance curve. This is the only way that I will be able to implement new features, without worrying about multiple implementations of each feature for each version of `.NET` just to maintain compatibility.

The coming release of `.NET 9` and `C# 13` seem like the natural point to start this next chapter, I would've loved to play with the new extension functionality, but seems it will be delayed further. In any case, future features of this package will need to rely on future features from `.NET`, therefore it makes no sense to keep trying to add support for them in older versions as well.
