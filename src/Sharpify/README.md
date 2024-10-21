# Sharpify

A collection of high performance language extensions for C#

## Why Sharpify?

Sharpify is a collection of commonly used language extensions, that usually people re-write in each project, This is an alternative where you can use the same convenient functionality, and gain the performance and reliability of using a highly optimized and extensively tested library. Sharpify has minimal footprint and is fully AOT compatible, so it can be used virtually in any project.

## Features

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

## Installation

[![Nuget](https://img.shields.io/nuget/dt/Sharpify?label=Sharpify%20Nuget%20Downloads)](https://www.nuget.org/packages/Sharpify/)
> dotnet add package Sharpify

## Usage

### Collection Extensions

```csharp
bool IsNullOrEmpty<T>(this ICollection<T>? collection);
Span<T> AsSpan<T>(this List<T> list);
ref TValue GetValueRefOrNullRef<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull {};
ref TValue? GetValueRefOrAddDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, out bool exists) where TKey : notnull {};
void CopyTo<TKey, TValue>(this Dictionary<TKey, TValue> dict, KeyValuePair<TKey,TValue>[] array, int index) : where TKey : notnull {};
(KeyValuePair<TKey, TValue>[] rentedBuffer, ArraySegment<KeyValuePair<TKey,TValue>> entries) RentBufferAndCopyEntries<TKey,TValue>(this Dictionary<TKey,TValue> dict) where TKey : notnull {};
void ReturnBufferToSharedArrayPool<T>(this T[] arr);
void ReturnBufferToArrayPool<T>(this T[] arr, ArrayPool<T> pool);
T[] PureSort<T>(this T[] source, IComparer<T> comparer);
List<T> PureSort<T>(this IEnumerable<T> source, IComparer<T> comparer);
void RemoveDuplicatesFromSorted<T>(this List<T> list, IComparer<T> comparer);
void RemoveDuplicates<T>(this List<T> list, IEqualityComparer<T>? comparer = null, bool isSorted = false);
void RemoveDuplicates<T>(this List<T> list, out HashSet<T> hSet, IEqualityComparer<T>? comparer = null, bool isSorted = false);
List<ArraySegment<T>> ChunkToSegments<T>(this T[] arr, int sizeOfChunk);
int CopyToArray<T>(this HashSet<T> hashSet, T[] destination, int index);
```

### Unmanaged Extensions

```csharp
bool TryParseAsEnum<TEnum>(this string value, out TEnum result) where TEnum : struct, Enum;
```

### String Extensions

```csharp
ref char GetReference(this string text); // Returns actual reference to first character
bool IsNullOrEmpty(this string str);
bool IsNullOrWhiteSpace(this string str);
// Tried to convert the string to int32 using ascii, very efficient
bool TryConvertToInt32(this ReadOnlySpan<char> value, out int result);
// Concat - more efficient than + or interpolation
string Concat(this string value, ReadOnlySpan<char> suffix);
string ToTitle(this string str);
bool IsBinary(this string str);
```

### Utils

Utils is a static class that provides advanced options that wouldn't be straight forward as extensions, it has static subclasses that are classified by their area of operation.

#### Utils.DateAndTime

```csharp
// Returns the date time as value task that can be fired and awaited to be later used, removing the need to synchronously wait for it
ValueTask<DateTime> GetCurrentTimeAsync();
// Same but returns the binary representation
ValueTask<long> GetCurrentTimeInBinaryAsync();
// Formats time span into a span of characters and returns the written portion
ReadOnlySpan<char> FormatTimeSpan(TimeSpan timeSpan, Span<char> buffer);
// Formats the time span into a string
string FormatTimeSpan(TimeSpan timeSpan);
// Formats the time stamp into a span of characters and returns the written portion
ReadOnlySpan<char> FormatTimeStamp(DateTime time, Span<char> buffer);
// Formats the time stamp into a string
string FormatTimeStamp(DateTime time);
```

#### Utils.Env

```csharp
bool IsRunningOnWindows();
bool IsRunningAsAdmin();
string GetBaseDirectory();
bool IsInternetAvailable;
string PathInBaseDirectory(string filename); // Returns a filename combined with the base directory
void OpenLink(string url); // semi-cross-platform (works on Windows, Mac and Linux)
```

#### Utils.Mathematics

```csharp
double RollingAverage(double oldAverage, double newNumber, int sampleCount);
double Factorial(double n);
double FibonacciApproximation(int n);
```

#### Utils.Strings

```csharp
// Format bytes into to a text containing the largest storage unit with 2 decimals places and the storage unit
// i.e 5.23 MB or 6.77 TB and so on...
string FormatBytes(double bytes);
string FormatBytes(long bytes);
// Formats the bytes in the same format to a buffer and returns the written span
ReadOnlySpan<char> FormatBytes(double bytes, Span<char> buffer);
ReadOnlySpan<char> FormatBytes(long bytes, Span<char> buffer);
```

#### Utils.Unsafe

```csharp
// Creates converts a predicate to a function that returns an integer value 0 or 1
Func<T, int> CreateIntegerPredicate<T>(Func<T, bool> predicate);
// Converts a readonly span to a mutable span
unsafe Span<T> AsMutableSpan<T>(ReadOnlySpan<T> span);
// Tries to unbox a valuetype from the heap
bool TryUnbox<T>(object obj, out T value) where T : struct;
```

### Union Types

This library contains 2 discriminate union types, to suit a wide variety of user needs.

#### 1. `Result` / `Result<T>`

This type works by having a `bool IsOk` property, a `string Message` and if needed a `T? Value` property. This allows the type to remain the same regardless of error or success, thus it is performs way better than lambda-required-handling types, but is more suited when the consumer knows exactly how it works.

For example, when the `Result<T>` is a failure, `T? Value` will be `null` this means, that if someone skips the check or tries to access the value when the `Result<T>` is a failure, they will get an exception.

* Both types are readonly structs but will throw an `InvalidOperation` exception if they are created using a default constructor. The only valid way to create them is using the static factory methods inside `Result`.
* Both `Result` and `Result<T>` also have the methods `.AsTask()` and `.AsValueTask()` that wrap a `Task` or `ValueTask` around them to make them easier to use in non-async `Task` or `ValueTask` methods.
* `Result` has an extension method called `.WithValue(T Value`, which will return a `Result<T>` with the same `Message` and `IsOk` values. However, it is not recommended to use as the performance is worse than the factory methods, and it allows adding a non-`null` `Value` to a failed `Result` which messes with the logic flow.

#### 2. `Either<T0, T1>`

This type is your usual lambda-required-handling discriminated union type, similar to `OneOf`. However it only has an option for 2 types.

This type has implicit operators that cast any of `T0` or `T1` to the type, and requires the consumer to either use delegates to get access to each, or to force casting it one type or the other. As with `OneOf` this makes it a little bit safer to use but vastly impacts performance, especially if you need to take the output value of one of them and continue processing it outside the lambda, or if you want to propagate a certain result forward in the code flow.

### ThreadSafe

`ThreadSafe<T>` is a special wrapper instance type that can make any other type thread-safe to be used in concurrency.

It works by having a lock and limiting modification access to a single thread at a time.

You can access the value any time by using `ThreadSafe.Value`.

and modify the value using the following method:

```csharp
public T Modify(Func<T, T> modificationFunc)
```

This both modifies the value and returns the result after modification.

### AesProvider

`AesProvider` is a class that implements `IDisposable` and allows very easy usage of AES128.

#### Static Methods

```csharp
string GeneratePassword(string password, int iterations = 991);
```

This generates a hashed password from a real password. This is useful for storing and verifying account credentials.

```csharp
bool IsPasswordValid(string password, string hashedPassword);
```

This will verify a hashed password against a real password.

#### Instance Methods

Constructor:

```csharp
public AesProvider(string strKey);
```

Unlike the base classes of the language, this takes a key in the format of a `string`, and does all the magic of handling length, padding and etc, by itself, so that you only need the key to encrypt or decrypt.

Remember that this class implements `IDisposable`, make sure to dispose of it properly or use the `using` keyword or block.

These methods handle regular encryption:

```csharp
// Encryption
string Encrypt(ReadOnlySpan<char> unencrypted);
string EncryptUrl(string url);
byte[] EncryptBytes(ReadOnlySpan<byte> unencrypted);
int EncryptBytes(ReadOnlySpan<byte> unencrypted, Span<byte> destination);
// Decryption
string Decrypt(string encrypted);
string DecryptUrl(string encryptedUrl);
byte[] DecryptBytes(ReadOnlySpan<byte> encrypted, bool throwOnError = false);
int DecryptBytes(ReadOnlySpan<byte> encrypted, Span<byte> destination, bool throwOnError = false);
```

These methods handle base64 encryption (for usage with URLs, filenames and etc.):

```csharp
string EncryptUrl(string url);
string DecryptUrl(string encryptedUrl);
```

For more advanced encryption or decryption you can use these:

```csharp
ICryptoTransform CreateEncryptor();
ICryptoTransform CreateDecryptor();
```

These will create an `ICryptoTransform` using the key and configuration from the `AesProvider` instance. You can use it to encrypt and decrypt using streams and many more advanced options.

`ICryptoTransform` is also implementing `IDisposable` make sure to handle it appropriately.

### Collections

Sharpify has multiple custom collections such as:

#### SortedList{T}

`SortedList<T>` is a re-implementation of `List<T>` with custom crud operations:

* Add -> O(log n)
* Remove -> O(log n)
* Get by sorted index O(1) - i.e min is [0] and max is [length - 1], also second max is [length - 2]...
* Option to disallow duplicates

The `SortedList<T>` also has convenience features, such as `AsSpan`, `Clear` methods, exposure of the `List<T>.Enumerator` which is an efficient struct, and also an implicit operator which can return the inner list in places which require `List<T>` (however be careful as the receiver may use the inner list and it may no longer maintain the features above)

#### PersistentDictionary

`PersistentDictionary` is a thread-safe `Dictionary<string, string` that is optimized for concurrency. The abstract class provides most of the important implementation to allow all the features, and should be used as the type for the object when you want to use a `PersistentDictionary`.

`PersistentDictionary` has many convenience methods such as automatic conversions that allow getting any value that implements `IParsable` and adding any value that implements `IConvertible` which at the vary least are most of the primitive types in .NET

The main differences between the api of this and a regular dictionary is that it is best to use the async overloads

```csharp
// Upsert
public ValueTask UpsertAsync<T>(string key, T value) where T : struct, IConvertible
public virtual async ValueTask UpsertAsync(string key, string value)
// Retrieval
public async ValueTask<T> GetOrCreateAsync<T>(string key, T @default) where T : struct, IParsable<T>
public virtual async ValueTask<string> GetOrCreateAsync(string key, string @default)
```

you can also get values with a synchronous operation if you require by using `PersistentDictionary[key]`

but upsert is not available asynchronously due to the synchronization mechanisms that are used to optimize the concurrency

To configure the type for usage you can implement the class and it will show you the specific things required to make everything work. In addition, there are 2 built-in implementations:

* `LocalPersistentDictionary` is an implementation that serializes and restores the dictionary from a local path
* `LazyLocalPersistentDictionary` is an implementation that also serializes and restores the dictionary from a local path, doesn't maintain an in-memory version, allowing it to be garbage collected if it was even created, this is for very memory constrained scenarios. Reading from it, doesn't even create a dictionary.

#### StringBuffer

`StringBuffer` is a ref struct that encapsulates a `Span{char}` and allow very efficient appending of characters, strings and other `ISpanFormattable` implementations. It enables usage patterns similar to that of `StringBuilder` but with a much lower memory footprint, and can work on stack allocated buffers.

It uses internal indexes to properly append elements, requiring no tracking from the user.

`StringBuffer` is created with a factory method named `Create(Span<char>)` that creates and returns an instance of `StringBuffer` with the specified buffer.

As it is a `ref struct`, it does have a default constructor, using it will create an instance on an empty buffer, that will throw an exception if you try to append anything to it. Please refrain from using it, it is only public because of compiler limitations.

##### Appending

```csharp
ref TBuffer Append(char c);
ref TBuffer Append(ReadOnlySpan<char> str);
ref TBuffer Append<T>(T value, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable {}
ref TBuffer AppendLine();
ref TBuffer AppendLine(char c);
ref TBuffer AppendLine(ReadOnlySpan<char> str);
ref TBuffer AppendLine<T>(T value, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable {};
```

All the append methods return a reference to self, this is to enable usage of the builder pattern.

##### Finalization

```csharp
// Will return the written portion of the buffer
WrittenSpan;
// Will create a string from the written portion of the buffer while removing end white spaces if they exist
Allocate(bool trimIfShorter, bool trimEndWhiteSpace);
Allocate(bool trimIfShorter); // ~ trimEndWhiteSpace = false
Allocate(); // ~ trimIfShorter = true, trimEndWhiteSpace = false
ToString() // Will call Allocate(true, false)
```

##### Example

```csharp
public string GetHello() {
  var buffer = StringBuffer.Create(stackalloc char[50]);
  buffer.Append("Hello");
  buffer.Append(' ');
  buffer.Append("Everyone");
  buffer.Append('!');
  return buffer.Allocate();
  // We sample text is separated for api showcase.
}
// The returned result will be "Hello Everyone!"
```

```csharp
// Example of usage with the builder pattern - similar to StringBuilder
public string GetHello() {
  return StringBuffer.Create(stackalloc char[50])
                           .Append("Hello")
                           .Append(' ')
                           .Append("Everyone")
                           .Append('!')
                           .Allocate();
}
```

#### RentedBufferWriter{T}

`RentedBufferWriter{T}` is an allocation friendly alternative to `ArrayBufferWriter{T}` which implements `IBufferWriter{T}`, an interface that represent a bucket that data can be written to. while it is not a commonly used interface, created to optimize specific hot paths, such as networking and IO pipes, using them is not very straightforward, and while `ArrayBufferWriter{T}` is a rather useful tool for some cases, it's limitation is that it isn't bound to any capacity, thus it always allocates arrays, and when it runs out of space, it allocates bigger arrays to resize, and that puts unneeded pressure of the GC.

`RentedBufferWriter{T}` fixes this by restricting the capacity at initialization, and renting the buffer from the shared array pool. Note that `SizeHint` in `GetSpan` and `GetMemory` is completely ignored in this implementation as resizing the inner buffer is currently not possible, by design. In case you are not sure what can exact capacity needed is, overestimate, it won't have much negative effects on the shared array pool.

Aside from implementing the interface `IBufferWriter{T}`, it also explicitly implements `IDisposable` to make sure the inner buffer is returned to the shared array pool after use. And implements many convenience methods and properties, such as:

```csharp
int ActualCapacity;
int FreeCapacity;
int Position { get; private set; }
ReadOnlySpan<T> WrittenSpan;
ReadOnlyMemory<T> WrittenMemory;
ArraySegment WrittenSegment;
ReadOnlySpan<T> GetSpanSlice(int start, int length);
ReadOnlyMemory<T> GetMemorySlice(int start, int length);
void Advance(int count);
bool WriteAndAdvance(T item);
bool WriteAndAdvance(ReadOnlySpan<T> data);
void Reset();
T[] Buffer; // Which returns the instance of the inner buffer, be careful with this.
ref T[] GetReferenceUnsafe(); // returns the reference for the inner buffer, be extra careful with this
```

### Routines

Routines are special objects that contain a collection of actions and executes them once a specified interval has passed, until it is stopped. This was inspired by Coroutines from the Unity game engine.

#### Routine

The default `Routine` is the simplest version, it has a collection of `Action`s and once every timer interval has passed they get executed sequentially.

#### AsyncRoutine

`AsyncRoutine` is more complicated but more precise and flexible, by providing a `CancellationTokenSource`, the `AsyncRoutine` can create tasks that will all run only while the source isn't cancelled.

In addition it can be initialized with a `RoutineOptions` that configures 2 important parameters:

1. `ThrowOnCancellation` will cause an exception to be thrown when the source is cancelled, if left out, the routine will gracefully cancel all the tasks and exit quietly.
2. `ExecuteInParallel` will configure the routine to execute the tasks concurrently and not sequentially which is the default.

### Special Types

#### SerializableObject{T}

`SerializableObject{T}` is a wrapper around a reference or value type, that will serialize the inner value to a file, monitor the file to synchronize external changes, and notify of changes via an `OnChanged` event.

The simplest use-case of this is for example you create a `record` for your app settings, which then enables each setting to be type safe and specific. Then when you change it from code.

A `JsonTypeInfo<T>` for the type is required, it will makes it more performant and AOT compatible.

##### Initialization

```csharp
new SerializableObject(string path, T defaultValue, JsonTypeInfo<T> jsonTypeInfo);
new SerializableObject(string path, JsonTypeInfo<T> jsonTypeInfo); // uses the other constructor with the default{T}
```

The constructor first validates the path, if the directory doesn't exist or filename is empty, it will throw a `IOException`, if the file doesn't exist, or the contents of the file are empty, it will serialize the default value to the file, otherwise it will deserialize from the file or set to the default if it fails.

In case you never created a `JsonSerializerContext`, this is how:
imagine for the example that the object type is `Configuration`

```csharp
// This needs to be under the namespace, it cannot be a nested class.
[JsonSourceGenerationOptions(WriteIndented = true)] // Optional
[JsonSerializable(typeof(Configuration))]
internal partial class JsonContext : JsonSerializerContext { }
// The source generator will take care of everything.

// Now an example of creating the object
public static readonly SerializableObject<Configuration> Config = new(_path, JsonContext.Default.Configuration);
// Notice how we passed the JsonContext
```

##### Modification

```csharp
void Modify(Func<T, T> modifier)
```

Modification is done using a function, this is to both enable an experience similar to `options` and to make it work with `struct`s because they are value types.

```csharp
Modify(person => {
  person.Name = "New";
  return person;
}); // Simple change that will work with reference types or value types
// If person was a record, it is even easier
Modify(person => person with { Name = "New" });
```

##### Subscribing And Notifications

The event that notifies for changes is `OnChanged`, and you need to subscribe to it with a signature of `void Function(object sender, SerializedObjectEventArgs e)`, this is a special event args implementation that will contain the new value after the change. an anonymous function with the same parameters is also accepted.

For example:

```csharp
var serializedObj = new SerializableObject(path, new Person { Name = "Dave" }, JsonSerializerContext.Default.Person);
monitoredObj.OnChanged += OnValueChanged

private void OnValueChanged(object sender, SerializedObjectEventArgs e) {
  Console.WriteLine($"The new name is {e.Value.Name}");
}
```

This basically concludes the general usage.

#### MonitoredSerializableObject{T}

`MonitoredSerializableObject{T}` is an extension of `SerializableObject{T}` which adds functionality of watching the filesystem to synchronize external changes, usage is basically identical except `MonitoredSerializableObject{T}` also implements `IDisposable` to release the resources of the file system watcher.

### Notes

* In order to avoid file writing exceptions, `Modify` is synchronized using a lock, to only be performed by a single thread at a time.
* There is also an internal mechanism that should prevent deserialization after internal modification in order to reduce io operations and redundant value updates.
* Both variants of `SerializableObject{T}` implement `IDisposable` and should be disposed of properly, but their main use-case is to be initialized once and used throughout the lifetime of the application, so this isn't absolutely crucial, and they both implement a finalizer that will dispose of the resources anyway.

## Contact

For bug reports, feature requests or offers of support/sponsorship contact <dusrdev@gmail.com>

> This project is proudly made in Israel 🇮🇱 for the benefit of mankind.
