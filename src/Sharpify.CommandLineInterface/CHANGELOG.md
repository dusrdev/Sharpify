# CHANGELOG

## Version 1.3.0

* `Arguments` now contains new methods `TryGetValues` and `TryGetValues{T}` to get arrays from values, there are overloads for regular and positional arguments, each overload requires a `string? separator` that is used to split the value, as with te regular values, `T` needs to implement `IParsable{T}`.
* `CliBuilder` now has a method `ShowErrorCodes` that will enable the error codes next to `CliRunner` error outputs, that was previously enabled by default, now it will hide them by default to provide a cleaner experience for users, but the builder now can easily configure this for testing, or if you still want the user to see them.

## Version 1.2.2

* Rewritten core function of argument forwarding to fix issue that caused non-positional arguments to be removed, now named arguments and flags should not be affected by positional forwarding at all.
  * Important note: the `Args` array that is stored within the `Arguments` object, is never modified and no matter how many positional forwarding iterations have been executed, it maintains the original arguments.
* Added `Arguments.HasFlag(string)` method that could be used to specifically checks for flags.
  * Previously `Arguments.Contains(string)` could be used for this purpose, but it could also return `true` for a named argument, effectively allowing a false-positive. `HasFlag` prevents this by checking that if it exists, the value is empty, which could only be the case for flags.
* Increased buffer size for help-text generation to prevents issues with complex clis.

### Usage Note

In case you are writing a cli which has a complex tree to navigate on the way to the execution, such as nested commands, and any single command processing gets verbose, remember that it is possible to create a `CliRunner` at any point.

This means that you can create objects for the nested commands, inside the top level command you could then forward the positional arguments (or not) if you choose, then use the same builder pattern with `CliRunner.CreateBuilder()...` and add the nested commands, then execute using the already parsed `Arguments` object as the `CliRunner.RunAsync` also has an overload that accepts `Arguments`.

## Version 1.2.1

* Updated core to use `Sharpify` 2.0.0
* small optimizations

## Version 1.2.0

* `Arguments`'s internal copy of the parsed args is now an array, this change was necessary to avoid special cases where the backing array was garbage collected leaving a phantom view. To get a read only copy you can use `.ArgsAsSpan` or `.ArgsAsMemory` according to your preference or use case.
* Improved `Parser`'s mapping function's stability, and also further reworked it to allow positional arguments after named ones, now positional arguments can be anywhere.
  * A special case that needs consideration before usage is switches, i.e boolean toggle parameters, as they look like named parameters without values. If such "switch" is followed by a regular value, it will be regarded as a named parameter and its value, as opposed to a switch and a positional argument. Keep this in mind when you decide the arrangement of input arguments, to ensure your input works as intended.
  * Switches work well, either when they are followed by other named arguments, or other switches. For simplicity, it is best to leave them as the last arguments.
* Added a new `SynchronousCommand` as an alternative to `Command`, it is basically syntactic sugar that makes it so you can implement an `Execute` method instead, in which you can return an `int`, when `async` is not needed, this can save multiple lines of code that just wrap `int`s in `ValueTask.FromResult` which can be quite verbose.

## Version 1.1.0

### Changes to `CliBuilder`

* `DoNotIncludeMetadataInHelpText` was removed, instead it will not be included by default. `ModifyMetadata` was renamed to `WithMetadata` and if used, will modify the default `CliMetadata` and include it in the help text.
* Added `WithCustomHeader(string)` as an alternative to using `CliRunnerMetadata`, there will be no exception when both are used, but in that case, `CliRunnerMetadata` has priority and will be the only one displayed.
* Added `SortCommandsAlphabetically`, which if specified will sort the commands alphabetically by name in the general help text, other than the help text, it has virtually no affect. Not specifying this, gives you control over the order, it will be exactly in the order that you added the commands and order of existing collection (if you added any commands via a collection).

### Changes to `Arguments`

* Overloads of `TryGetValue<TEnum>` were modified to add an option to `ignoreCase`, to make it more user friendly and still adhere to parameter placement guidelines, more overloads were added.

## Version 1.0.5

* Added a `ReadOnlyMemory{string}` which is a copy of the arguments split up before being parsed to `Arguments`, it can be retrieved by the `Arguments.PureArguments`, in special cases in which you might create a nested command structure, which requires a partial parsing, then secondary parsing within a command, this can be very powerful as you can create a secondary `CliRunner` and pass any subsequence of those arguments to recreate an input.
* Overloads of `Arguments.GetValue` which take an `int` as `positional argument`, now that parameter renamed to be `position` to better signify what the overloads mean, it is a rather cosmetic change, but nevertheless.
* Add a `Arguments.Contains(int)` overload to match with the rest of the methods and suit `positional arguments`.

## Version 1.0.4

* Added missing line break in global help text
* If the single word help is entered, it will now be recognized in place of command name to return the global help text, instead of trying to be parsed as a command.

## Version 1.0.3

* Updated `Sharpify` dependency and implemented usage of new APIs to aid in maintainability.
* Add `DoNotIncludeMetadataInHelpText()` in `CliBuilder` which removes the metadata inclusion in the general help text.

## Version 1.0.2

* Removed thread-local `StringBuilder` from `CliRunner`, replaced all usages with `StringBuffer` from `Sharpify`

## Version 1.0.1

* Updated `Sharpify` dependency
* Slightly improved performance of general help text generator

## Version 1.0.0

Initial version - no changes
