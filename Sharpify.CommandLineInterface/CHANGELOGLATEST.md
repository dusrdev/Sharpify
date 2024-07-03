# CHANGELOG

## Version 1.2.2

* Rewritten core function of argument forwarding to fix issue that caused non-positional arguments to be removed, now named arguments and flags should not be affected by positional forwarding at all.
  * Important note: the `Args` array that is stored within the `Arguments` object, is never modified and no matter how many positional forwarding iterations have been executed, it maintains the original arguments.
* Added `Arguments.HasFlag(string)` method that could be used to specifically checks for flags.
  * Previously `Arguments.Contains(string)` could be used for this purpose, but it could also return `true` for a named argument, effectively allowing a false-positive. `HasFlag` prevents this by checking that if it exists, the value is empty, which could only be the case for flags.
* Increased buffer size for help-text generation to prevents issues with complex clis.

### Usage Note

In case you are writing a cli which has a complex tree to navigate on the way to the execution, such as nested commands, and any single command processing gets verbose, remember that it is possible to create a `CliRunner` at any point.

This means that you can create objects for the nested commands, inside the top level command you could then forward the positional arguments (or not) if you choose, then use the same builder pattern with `CliRunner.CreateBuilder()...` and add the nested commands, then execute using the already parsed `Arguments` object as the `CliRunner.RunAsync` also has an overload that accepts `Arguments`.
