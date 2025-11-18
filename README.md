# Sharpify

[![NuGet](https://img.shields.io/nuget/v/Sharpify.svg?style=flat-square)](https://www.nuget.org/packages/Sharpify)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Sharpify?style=flat&label=Downloads)](https://www.nuget.org/packages/Sharpify)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](License.txt)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

A collection of high performance language extensions for C#, fully compatible with NativeAOT

* ⚡ Fully Native AOT compatible
* 🤷 `Either<T0, T1>` - Discriminated union object that forces handling of both cases
* 🦾 Flexible `Result` type that can encapsulate any other type and adds a massage options and a success or failure status. Flexible as it doesn't require any special handling to use (unlike `Either`)
* 🏄 Wrapper extensions that simplify use of common functions and advanced features from the `CollectionsMarshal` class
* `Routine` and `AsyncRoutine` bring the user easily usable and configurable interval based background job execution.
* `SortedList<T>` bridges the performance of `List` and order assurance of `SortedSet`
* `Synchronized<T>` is a thread-safe object owner with an optional delegate that can be executed on update.
* 💿 `StringBuffer` enables zero allocation, easy to use appending buffer for creation of strings in hot paths.
* `PooledArrayOwner{T}` is struct based alternative to `IMemoryOwner<T>` with extensions built into the `ArrayPool<T>` class.
* `BufferWrapper{T}` is a ref struct implementation of `IBufferWriter{T}` that wraps a `Span<T>`.
* A 🚣🏻 boat load of extension functions for all common types, bridging ease of use and performance.
* A bunch of utils in `Utils` class.
* 🔐 `AesProvider` provides access to industry leading AES-128 encryption with virtually no setup
* 🏋️ High performance optimized alternatives to core language extensions
* 🫴 Focus on giving the user complete control by using flexible and common types, and resulting types that can be further used and just viewed.

## ⬇ Installation

> dotnet add package Sharpify

## Sharpify.CommandLineInterface

`Sharpify.CommandLineInterface` is a standalone package that adds a high performance, reflection free and `AOT-ready` framework for creating command line and embedded interfaces

It was moved to its own repo [Sharpify.CommandLineInterface](https://github.com/dusrdev/Sharpify.CommandLineInterface)

##

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
