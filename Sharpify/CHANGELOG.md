# CHANGELOG

## v1.0.4

* Added url encryption and decryption functions to `AesProvider`
* Added more safeguards that prevent exceptions when `plain` text input is passed as `encrypted` to decryption functions. They now return an empty output, either `byte[]` or `string.Empty`, depends on the method.

## v1.0.3

* Fixed implementation of `IModifier<T>` to better fit the requirements of `Func<T, T>`

## v1.0.2

* Introduces a new `ThreadSafe<T>` wrapper which makes any type thread-safe
* Introduces a new `AesProvider` class with enables no-setup access to encryption

### `ThreadSafe<T>` Notes

* Access the value by using `ThreadSafe<T>.Value`
* Modify the value by using `ThreadSafe<T>.Modify(Func<T, T>)` or `ThreadSafe<T>.Modify(IModifier<T>)`
* `IModifier<T>` allows you to create an operation that will be better optimized than a `Func<T, T>` and possibly avoid the memory allocation penalty associated with `delegates`
* Unlike the `Interlocked` api's, which require using `ref` thus making them unusable in `async` methods, `ThreadSafe<T>` doesn't, which makes it much more usable

### `AesProvider` Notes

* Has `string` key based constructor that takes care of the headache for padding and key size for you
* Has methods for encrypting and decrypting both `string`s and `byte[]`s
* Provides an option to generate an `ICryptoTransform` for either encryption or decryption to fit special needs
* Properly implements `IDisposable` with api notices to prevent memory leaks
* Has static methods for generating hashed passwords and validating them

## v1.0.1

* `Utils` class was made upper class of `Env`, `Mathematics` and `DateAndTime` to allow better categorization and maintenance
* Fixed invalid options in the .csproj file

### `Utils.Env`

* Added `GetBaseFolder` which returns the base path of the application directory
* Added `IsRunningAsAdmin` and `IsRunningOnWindows` which are self-explanatory
* Added `IsInternetAvailable` which checks for internet connection

### `Utils.Mathematics`

* Added `FibonacciApproximation` and `Factorial` functions
