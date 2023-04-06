# Sharpify

A collection of high performance language extensions for C#

## Features

* Discriminated union object that forces handling of both cases called `Either<T0, T1>`
* Flexible `Result` type that can encapsulate any other type and adds a massage options and a success or failure status. Flexible as it doesn't require any special handling to use (unlike `Either`)
* `Concurrent` wrapper object together with `IAction` and `IAsyncAction` interfaces and extension methods that are a window into vastly improved parallel/concurrent processing (More info below)
* Utility functions that add new functionality or change the way you access certain features (More info below).
* Large collection of extension methods that replace core library functions with alternatives that offer tremendously improved performance.

## How was any of it achieved and why

From my experience with coding with C# for about a decade now, I was used to seeing an extension class in almost any project, often with extensions that are almost trivial that should exist in the core language, others that are very smart and intuitive and could be added to increase performance and/or productivity, This is why I thought it would be amazing if there could be a lightweight package to just install in all projects that would fill all those needs. In the last few years the .NET team has made a lot of efforts to optimize code, add new features and a lot more. At this time (Latest release is .NET 7) the features are still better than what is available in the core language. Staying updated in the official additions to the language, I will update the package and mark features as deprecated if a better official one will be released.

To achieve the features the package:

* Uses `Span`, `ReadOnlySpan` and `stackalloc` pretty heavily.
* `Result` struct doesn't require handling with **lambdas** to virtually eliminate boxing and heap allocation.
* `IAction` and `IAsyncAction` provide more readable and configurable alternatives to lambda functions that are also subject to a large degree of optimization using JIT, and together with **ref struct** `Concurrent` wrapper, allow usage of incredibly performant concurrent processing extensions.

### More on `Concurrent`

The interfaces `IAction` and `IAsyncAction` allow usage of **readonly structs** to represents the actual **lambda** function alternative, in addition of possibly being allocated on the stack, it also allows usage of readonly field and provides clarity for the **JIT** compiler allowing it to optimize much more during runtime than if it were **lambda** functions. The `Concurrent` wrapper serves 3 purposes: first is separating the extensions of this library from the rest of parallel core extensions, to make sure you really are using the one you want. Second is to limit actual types of collections you could use, In order to maximize the performance only collections that implement `ICollection<T>` can be used. Third is that wrapping the collection as a **field** in a **ref struct** sometimes helps allocate more of the actual processing dependencies on the stack, and most of the time even if not, it will allocate the pointer to the stack which will help the **JIT** to further optimize during runtime.

### More on the `Utils` class

* Adds an option for calculating **rolling average** for `double`
* Adds new interfaces to access `DateTime.Now` using `GetCurrentTimeAsync` and `GetCurrentTimeInBinaryAsync`, as using the default ones involves a system call and it is blocking, it actually takes quite a bit of time, from my testing about 180ns, the new functions allow calling to a variable and not awaiting them, then later awaiting the actual variable to either get the value or wait for it to complete and get the value. This allows you to actually do other things while awaiting this system call. It can make a big difference in high-performance scenarios where you use `DateTime.Now to also get a timestamp