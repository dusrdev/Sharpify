# CHANGELOG

## Version 1.0.5

* Added a `ReadOnlyMemory{string}` which is a copy of the arguments split up before being parsed to `Arguments`, it can be retrieved by the `Arguments.PureArguments`, in special cases in which you might create a nested command structure, which requires a partial parsing, then secondary parsing within a command, this can be very powerful as you can create a secondary `CliRunner` and pass any subsequence of those arguments to recreate an input.
* Overloads of `Arguments.GetValue` which take an `int` as `positional argument`, now that parameter renamed to be `position` to better signify what the overloads mean, it is a rather cosmetic change, but nevertheless.
* Add a `Arguments.Contains(int)` overload to match with the rest of the methods and suit `positional arguments`.
