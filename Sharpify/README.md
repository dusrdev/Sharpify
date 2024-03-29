# Sharpify

A collection of high performance language extensions for C#

## Features

* ⚡ Fully Native AOT compatible
* 🤷 `Either<T0, T1>` - Discriminated union object that forces handling of both cases
* 🦾 Flexible `Result` type that can encapsulate any other type and adds a massage options and a success or failure status. Flexible as it doesn't require any special handling to use (unlike `Either`)
* 🚀 Extremely efficient concurrency with `Concurrent` collection wrapper and `IAction`/`IAsyncAction` interfaces, and even more efficient with `AsyncLocal` optimized APIs.
* 🏄 Wrapper extensions that simplify use of common functions and advanced features from the `CollectionsMarshal` class
* `Routine` and `AsyncRoutine` bring the user easily usable and configurable interval based background job execution.
* `PersistentDictionary` and derived types are super lightweight and efficient serializable dictionaries that are thread-safe and work amazingly for things like configuration files.
* `SortedList<T>` bridges the performance of `List` and order assurance of `SortedSet`
* `PersistentDictionary` and variants provide all simple database needs, with perfected performance and optimized concurrency.
* `SerializableObject` and the `Monitored` variant allow persisting an object to the disk, and elegantly synchronizing modifications.
* 💿 `StringBuffer` and `AllocatedStringBuffer` enable zero allocation, easy to use appending buffer for creation of string in hot paths.
* `RentedBufferWriter{T}` is an allocation friendly alternative to `ArrayBufferWriter{T}` for hot paths.
* A 🚣🏻 boat load of extension functions for all common types, bridging ease of use and performance.
* `Utils.Env`, `Utils.Math`, `Utils.Strings` and `Utils.Unsafe` provide uncanny convenience at maximal performance.
* 🧵 `ThreadSafe<T>` makes any variable type thread-safe
* 🔐 `AesProvider` provides access to industry leading AES-128 encryption with virtually no setup
* 🏋️ High performance optimized alternatives to core language extensions
* 🎁 More added features that are not present in the core language
* ❗ Static inner exception throwers guide the JIT to further optimize the code during runtime.
* 🫴 Focus on giving the user complete control by using flexible and common types, and resulting types that can be further used and just viewed.

Check details out in the GitHub Wiki

## Contact

For bug reports, feature requests or offers of support/sponsorship contact <dusrdev@gmail.com>
