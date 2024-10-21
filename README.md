# Sharpify

A collection of high performance language extensions for C#, fully compatible with NativeAOT

## ⬇ Installation

[![Nuget](https://img.shields.io/nuget/dt/Sharpify?label=Sharpify%20Nuget%20Downloads)](https://www.nuget.org/packages/Sharpify/)
> dotnet add package Sharpify

[![Nuget](https://img.shields.io/nuget/dt/Sharpify.Data?label=Sharpify.Data%20Nuget%20Downloads)](https://www.nuget.org/packages/Sharpify.Data/)
> dotnet add package Sharpify.Data

[![Nuget](https://img.shields.io/nuget/dt/Sharpify.CommandLineInterface?label=Sharpify.CommandLineInterface%20Nuget%20Downloads)](https://www.nuget.org/packages/Sharpify.CommandLineInterface/)
> dotnet add package Sharpify.CommandLineInterface

## Sharpify - Base package

`Sharpify` is a package mainly intended to extend the core language using high performance implementations. The other 2 packages uses `Sharpify` as a dependency. But its core features can be useful in a variety of applications by themselves.

* ⚡ Fully Native AOT compatible
* 🤷 `Either<T0, T1>` - Discriminated union object that forces handling of both cases
* 🦾 Flexible `Result` type that can encapsulate any other type and adds a massage options and a success or failure status. Flexible as it doesn't require any special handling to use (unlike `Either`)
* 🏄 Wrapper extensions that simplify use of common functions and advanced features from the `CollectionsMarshal` class
* `Routine` and `AsyncRoutine` bring the user easily usable and configurable interval based background job execution.
* `PersistentDictionary` and derived types are super lightweight and efficient serializable dictionaries that are thread-safe and work amazingly for things like configuration files.
* `SortedList<T>` bridges the performance of `List` and order assurance of `SortedSet`
* `PersistentDictionary` and variants provide all simple database needs, with perfected performance and optimized concurrency.
* `SerializableObject` and the `Monitored` variant allow persisting an object to the disk, and elegantly synchronizing modifications.
* 💿 `StringBuffer` enables zero allocation, easy to use appending buffer for creation of strings in hot paths.
* `RentedBufferWriter{T}` is an allocation friendly alternative to `ArrayBufferWriter{T}` for hot paths.
* A 🚣🏻 boat load of extension functions for all common types, bridging ease of use and performance.
* `Utils.DateAndTime`, `Utils.Env`, `Utils.Math`, `Utils.Strings` and `Utils.Unsafe` provide uncanny convenience at maximal performance.
* 🧵 `ThreadSafe<T>` makes any variable type thread-safe
* 🔐 `AesProvider` provides access to industry leading AES-128 encryption with virtually no setup
* 🏋️ High performance optimized alternatives to core language extensions
* 🎁 More added features that are not present in the core language
* ❗ Static inner exception throwers guide the JIT to further optimize the code during runtime.
* 🫴 Focus on giving the user complete control by using flexible and common types, and resulting types that can be further used and just viewed.

For more information check [inner directory](src/Sharpify/README.md).

## Sharpify.Data

`Sharpify.Data` is an extension package, that should be installed on-top of `Sharpify` and adds a high performance persistent key-value-pair database, utilizing [MemoryPack](https://github.com/Cysharp/MemoryPack). The database support multiple types in the same file, 2 stage AES encryption (for whole file and per-key). Filtering by type, Single or Array value per key, and more...

* `Database` is the base type for the data base, it is key-value-pair based local database - saved on disk.
* `IDatabaseFilter<T>` is an interface which acts as an alternative to `DbContext` and provides enhanced type safety for contexts.
* `MemoryPackDatabaseFilter<T>` is an implementation which focuses on types that implement `IMemoryPackable<T>` from `MemoryPack`.
* `FlexibleDatabaseFilter<T>` is an implementation focusing on types which need custom serialization logic. To use this, you type `T` will need to implement `IFilterable<T>` which has methods for serialization and deserialization of single `T` and `T[]`. If you can choose to implement only one of the two.
* **Concurrency** - `Database` uses highly performant synchronous concurrency models and is completely thread-safe.
* **Disk Usage** - `Database` tracks inner changes and skips serialization if no changes occurred, enabling usage of periodic serialization without resource waste.
* **GC Optimization** - `Database` heavily uses pooling for encryption, decryption, type conversion, serialization and deserialization to minimize GC overhead, very rarely does it allocate single-use memory and only when absolutely necessary.
* **HotPath APIs** - `Database` is optimized for hot paths, as such it provides a number of APIs that specifically combine features for maximum performance and minimal GC overhead. Like the `TryReadToRentedBuffer<T>` methods which is optimized for adding data to a table.
* **Runtime Optimization** - Upon initialization, `Database` chooses specific serializers and deserializers tailored for specific configurations, minimizing the amount of runnable code during runtime that would've been wasted on different checks.

For more information check [inner directory](src/Sharpify.Data/README.md).

## Sharpify.CommandLineInterface

`Sharpify.CommandLineInterface` is another extension package that adds a high performance, reflection free and `AOT-ready` framework for creating command line and embedded interfaces

* Maintenance friendly model that depends on class that implement `Command` or `SynchronousCommand`
* `Arguments` is an abstraction layer over the inputs that validate during runtime according to user needs via convenient APIs.
* Configuration using a fluent builder pattern.
* Configurable output and input pipes, enable usage outside of `Console` apps, enabling the option for embedded use in any application.
* Automatic and structured general and command-specific help text.
* Configurable error handling with defaults.
* Super lightweight

For more information check [inner directory](src/Sharpify.CommandLineInterface/README.md)

## Methodology

* Backwards compatibility ❌
* Stability at release ✅

As the name suggests - `Sharpify` intends to extend the core language features using high performance implementations. `Sharpify` or its extension packages are not guaranteed to be backwards compatible, and each release may contain breaking changes as they try to adapt to the latest language features. `.NET` has a very active community and many features will be added to the core language that will perform at some point better than what `Sharpify` currently offers, at which point these features will be removed from `Sharpify` to encourage users to use the core language features instead.

The decision to disregard backwards compatibility is based on the idea to only provide feature that **add** or **improve** current language features. This is to ensure that both the package remains relevant, and unbounded by old sub-par implementations, and to encourage users to adapt their code to new language features.

Even thought backwards compatibility is not guaranteed, `Sharpify` has very high coverage of unit tests, and should be completely stable upon release. All issues will be treated as **Urgent**.

If your packages / libraries use `Sharpify`, and you don't want to modify the code often, I recommend locking the dependency to a specific version which you test.

## Contribution

This packages was made public so that the entire community could benefit from it. If you experience issues, want to suggest new features or improve existing ones, please use the [issues](https://github.com/dusrdev/Sharpify/issues) section.

## Contact

For bug reports, feature requests or offers of support/sponsorship contact <dusrdev@gmail.com>

> This project is proudly made in Israel 🇮🇱 for the benefit of mankind.
