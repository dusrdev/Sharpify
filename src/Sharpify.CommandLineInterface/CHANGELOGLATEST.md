# CHANGELOG

## Version 2.0.0

**WARNING:** This release may contain breaking changes.

- The `Arguments` source collection has been rewritten as a `ReadOnlyCollection<string>`, which cascaded into numerous changes:
  - `Parser.ParseArguments` (collection-based overloads) now takes a generic `IList<string>`. This is converted internally to a `ReadOnlyCollection<string>`, which is used as the source for `Arguments`.
  - As a result, `Parser.Split` now returns a `List<string>`.
  - The `Parser.SplitToList` method was removed, as it is no longer needed.
  - `Arguments.ArgsAsMemory` and `Arguments.ArgsAsSpan` were also removed. To inspect the source, use `Arguments.Source` or obtain a copy as a `string[]` with `Arguments.SourceCopy`.
  - The `CliRunner.RunAsync` overload that previously accepted a `ReadOnlySpan<string>` now accepts an `IList<string>` instead. This allows implicit casting from both `string[]` and `List<string>`, which are the most common CLI inputs.
- `HelpText` generators now use a `StringBuilder` internally, replacing the previous custom buffer. Since help text generation typically occurs only once during a CLI's lifetime, any potential performance impact is minimal. This also removes some logical size restraints.
- Removed `Microsoft.SourceLink.Github` as it is now used implicitly.

**These changes enable several improvements:**

- `Sharpify` is no longer a required dependency of this package and has been removed. This package can now be installed as a standalone.
- Creating an `Arguments` object with `Parser.ParseArguments` is now much simpler. You can use it directly without commands to create minimal CLIs in `Program.cs`. This will be particularly useful with the upcoming `.NET 10` feature allowing direct execution of `.cs` files. (A demo video with examples and best practices will be released when this feature is available.)
