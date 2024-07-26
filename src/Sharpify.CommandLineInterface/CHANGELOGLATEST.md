# CHANGELOG

## Version 1.3.0

* `Arguments` now contains new methods `TryGetValues` and `TryGetValues{T}` to get arrays from values, there are overloads for regular and positional arguments, each overload requires a `string? separator` that is used to split the value, as with te regular values, `T` needs to implement `IParsable{T}`.
* `CliBuilder` now has a method `ShowErrorCodes` that will enable the error codes next to `CliRunner` error outputs, that was previously enabled by default, now it will hide them by default to provide a cleaner experience for users, but the builder now can easily configure this for testing, or if you still want the user to see them.

### Usage Note

In case you are writing a cli which has a complex tree to navigate on the way to the execution, such as nested commands, and any single command processing gets verbose, remember that it is possible to create a `CliRunner` at any point.

This means that you can create objects for the nested commands, inside the top level command you could then forward the positional arguments (or not) if you choose, then use the same builder pattern with `CliRunner.CreateBuilder()...` and add the nested commands, then execute using the already parsed `Arguments` object as the `CliRunner.RunAsync` also has an overload that accepts `Arguments`.
