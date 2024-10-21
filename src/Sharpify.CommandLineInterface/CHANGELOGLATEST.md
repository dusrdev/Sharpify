# CHANGELOG

## Version 1.4.0

* Optimized `Parser`:
  * `Split` now rents a buffer the array pool by itself and returns a `RentedBufferWriter<string>`, this enables greater flexibility in usage, and simplifies the code.
  * Changed lower level array allocation code to use generalized api to optimize on more platforms.
* `Arguments.TryGetValue` and `Arguments.TryGetValues` now have overloads that accept a `ReadOnlySpan<string> keys`, this overload enables much simpler retrieval of parameters that have aliases, for example you might want something like `--name` and `-n` to map to the same value.
  * If you specify the aliases using the collections expression (i.e `["one", "two"]`), since .NET 8, the compiler will generate an inline array for that, which is very efficient, you don't need to create an array yourself. but if you wanted to to you could for example create a `static readonly ReadOnlySpan<string> aliases => new[] { "one", "two" };` and pass that instead, the compiler optimizes such case by writing the values directly in the assembly.
* `CliBuilder` now has an option `WithCaseSensitiveParameters` that will make the parser case sensitive, this is useful if you want to have parameters that are case sensitive, by default the parser is case insensitive. the decision to default to ignore case is centered around making it easier for users to use the cli. But for cases where you need more short flags like `grep` you can opt in for this feature.
* `CliBuilder` now has an option `WithoutHelpTextForEmptyInput` that will prevent the general help text from being displayed when no input is given, this is useful for cases where you want to have a more silent cli, by default the general help text is displayed when no input is given.
  * This is a change in behavior, as previously by default an error showing that no command was found was displayed, but seems that showing the help text in those situations is the more common approach in modern CLIs.
* Updated parsing to detect cases where arguments start with `-` and are not names of arguments, for example if you required a positional argument of type `int` and the input was a negative number (also starts with `-`), it would've been interpreted as a named argument, now it will be correctly interpreted as a positional argument.
  * The rule now also checks if the first character following a `-` is a digit, if it is, it will not be marked as named argument. Which means - don't use argument names that start with digits (this is a bad practice in general).
* Help text no contains a special case for "version" and "--version" that will just display the version from metadata.
  * Help text (from main) now has specialized structure for cases where you only have one command, instead of printing commands and descriptions, it will print the single command usage - the rest will of the whole cli (metadata)
* To support `--version` and add more customization options, now `Metadata` and `CustomHeader` are independent, and you can configure which is used for help text with `SetHelpTextSource(HelpTextSource)`. `Metadata` will be used by default.
* The help text portion that used to display instruction to get help text is now shorter and more concise.

### Usage Note

In case you are writing a cli which has a complex tree to navigate on the way to the execution, such as nested commands, and any single command processing gets verbose, remember that it is possible to create a `CliRunner` at any point.

This means that you can create objects for the nested commands, inside the top level command you could then forward the positional arguments (or not) if you choose, then use the same builder pattern with `CliRunner.CreateBuilder()...` and add the nested commands, then execute using the already parsed `Arguments` object as the `CliRunner.RunAsync` also has an overload that accepts `Arguments`.
