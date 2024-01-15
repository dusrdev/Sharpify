# CHANGELOG

* `Sharpify` is now fully AOT-Compatible!!
* Performed IO optimizations to `LocalPersistentDictionary` and `LazyLocalPersistentDictionary` and configured to use compile-time JSON.
* **BREAKING** `ReturnRentedBuffer` was renamed to `ReturnBufferToSharedArrayPool` to better explain what it does,
Also added a method `ReturnToArrayPool` which takes an `ArrayPool` in case anyone wanted an extension method to be used with custom `ArrayPool`. The main reason for both these extensions is because the `ArrayPool`s generic type is on the class and not the method, it usually can't be inferred, resulting in longer and more difficult code to write. The extension methods hide all of the generic types because they are made in a format which the compiler can infer from.
* **BREAKING** `SerializableObject` and `MonitoredSerializableObject` now require a `JsonSerializationContext` parameter, that makes them use compile time AOT compatible serialization. This was required in order to make the entire library AOT compatible.
