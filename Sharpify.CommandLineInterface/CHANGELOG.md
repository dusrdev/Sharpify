# CHANGELOG

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
