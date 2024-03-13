# CHANGELOG

## Version 1.2.0

* `Arguments`'s internal copy of the parsed args is now an array, this change was necessary to avoid special cases where the backing array was garbage collected leaving a phantom view. To get a read only copy you can use `.ArgsAsSpan` or `.ArgsAsMemory` according to your preference or use case.
* Improved `Parser`'s mapping function's stability, and also further reworked it to allow positional arguments after named ones, now positional arguments can be anywhere.
  * A special case that needs consideration before usage is switches, i.e boolean toggle parameters, as they look like named parameters without values. If such "switch" is followed by a regular value, it will be regarded as a named parameter and its value, as opposed to a switch and a positional argument. Keep this in mind when you decide the arrangement of input arguments, to ensure your input works as intended.
  * Switches work well, either when they are followed by other named arguments, or other switches. For simplicity, it is best to leave them as the last arguments.
* Added a new `SynchronousCommand` as an alternative to `Command`, it is basically syntactic sugar that makes it so you can implement an `Execute` method instead, in which you can return an `int`, when `async` is not needed, this can save multiple lines of code that just wrap `int`s in `ValueTask.FromResult` which can be quite verbose.
