# Sharpify

A collection of high performance language extensions for C#

## Features

* 🤷 `Either<T0, T1>` - Discriminated union object that forces handling of both cases
* 🦾 Flexible `Result` type that can encapsulate any other type and adds a massage options and a success or failure status. Flexible as it doesn't require any special handling to use (unlike `Either`)
* 🚀 Extremely efficient concurrency with `Concurrent` collection wrapper and `IAction`/`IAsyncAction` interfaces
* 🏄 Wrapper extensions that simplify use of common functions and advanced features from the `CollectionsMarshal` class
* `Routine` and `AsyncRoutine` bring the user easily usable and configurable interval based background job execution.
* `PersistentDictionary` and derived types are super lightweight and efficient serializable dictionaries that are thread-safe and work amazingly for things like configuration files.
* `SortedList<T>` bridges the performance of `List` and order assurance of `SortedSet`
* 🧵 `ThreadSafe<T>` makes any variable type thread-safe
* 🔐 `AesProvider` provides access to industry leading AES-128 encryption with virtually no setup
* 🏋️ High performance optimized alternatives to core language extensions
* 🎁 More added features that are not present in the core language
* ❗Parameter validation is handled with `Debug.Assert` statements instead of `Exception` throwing to increase performance in Release builds
* 🫴 Focus on giving the user complete control by using flexible and common types, and resulting types that can be further used and just viewed.

### More on `Concurrent`

The interfaces `IAction` and `IAsyncAction` allow usage of **readonly structs** to represents the actual **lambda** function alternative, in addition of possibly being allocated on the stack, it also allows usage of readonly field and provides clarity for the **JIT** compiler allowing it to optimize much more during runtime than if it were **lambda** functions. The `Concurrent` wrapper serves 3 purposes: first is separating the extensions of this library from the rest of parallel core extensions, to make sure you really are using the one you want. Second is to limit actual types of collections you could use, In order to maximize the performance only collections that implement `ICollection<T>` can be used. Third is that wrapping the collection as a **field** in a **ref struct** sometimes helps allocate more of the actual processing dependencies on the stack, and most of the time even if not, it will allocate the pointer to the stack which will help the **JIT** to further optimize during runtime.

### More on `Result`

`Result` is a `readonly record struct` that includes a `bool` status of either success or failure and an optional `string` message.
In addition to that there is an alternative `Result<T>` which can also store a value of type **T**. The result class uses static factory methods to create both `Result` and `Result<T>` objects, and implicit converters minimize complexity and unreadability of code.

Unlike `Either<T0, T1>`, `Result` does force the user to handle it in any special way, instead nullable properties are used. both `Result.Message` and `Result.Value` (if `Result<T>` is used) can be null, and the factory methods for `Fail` set the `Value` to null. So that in the worst case you only allocate a null reference.

All of these design choices guarantee vastly improved performance over `Either<T0, T1>` since, you can use any objects during the handling of the result, or pass the result entirely or just parts of it between methods without worrying of boxing and heap allocations from lambdas

### More on `ThreadSafe<T>`

* Access the value by using `ThreadSafe<T>.Value`
* Modify the value by using `ThreadSafe<T>.Modify(Func<T, T>)` or `ThreadSafe<T>.Modify(IModifier<T>)`
* `IModifier<T>` allows you to create an operation that will be better optimized than a `Func<T, T>` and possibly avoid the memory allocation penalty associated with `delegates`
* Unlike the `Interlocked` api's, which require using `ref` thus making them unusable in `async` methods, `ThreadSafe<T>` doesn't, which makes it much more usable

### More on `AesProvider`

* Has `string` key based constructor that takes care of the headache for padding and key size for you
* Has methods for encrypting and decrypting both `string`s and `byte[]`s
* Provides an option to generate an `ICryptoTransform` for either encryption or decryption to fit special needs
* Properly implements `IDisposable` with api notices to prevent memory leaks
* Has static methods for generating hashed passwords and validating them

### More on the `Utils` class

* Adds an option for calculating **rolling average** for `double`
* Adds new interfaces to access `DateTime.Now` using `GetCurrentTimeAsync` and `GetCurrentTimeInBinaryAsync`, as using the default ones involves a system call and it is blocking, it actually takes quite a bit of time, from my testing about 180ns, the new functions allow calling to a variable and not awaiting them, then later awaiting the actual variable to either get the value or wait for it to complete and get the value. This allows you to actually do other things while awaiting this system call. It can make a big difference in high-performance scenarios where you use `DateTime.Now to also get a timestamp

#### Contact

For bug reports, feature requests or offers of support/sponsorship contact <dusrdev@gmail.com>
