# Sharpify.CommandLineInterface

`Sharpify.CommandLineInterface` is a high performance, reflection free and AOT-ready framework for creating command line interfaces, with a configurable output writer and no direct dependency to `System.Console` enabling it to be embedded, used with inputs from any source and output to any source.

Most other command line frameworks in c# use `reflection` to provide their "magic" such as generating help text, and providing input validation, `Sharpify.CommandLineInterface` instead uses compile time implemented metadata and user guided validation. each command must implement the `Command` or `SynchronousCommand` abstract class, part of which will be to set the command metadata, the main entry `CliRunner` also has an application level metadata object that can be customized in the `CliBuilder` process, using those, `Sharpify.CommandLineInterface` can resolve and format that metadata to generate an output similar to the other frameworks. Each command's entry point is either `ExecuteAsync` or `Execute` which receive an input of type `Arguments` that can be used to retrieve, validate and parse arguments.

## Usage

### Implementing Commands

To implement a command create a class that inherits from the abstract `Command`:

```csharp
public sealed class EchoCommand : Command {
  public override string Name => "echo";

  public override string Description => "Echoes the specified message.";

  public override string Usage => "echo <message>";

  public override ValueTask<int> ExecuteAsync(Arguments args) {
    if (!args.TryGetValue("message", out string message)) { // Validation
      // This example returns error code 400 (http bad request code) to signal client error
      // Any code you want can obviously be used
      return OutputHelper.Return("No message specified", 400, true);
    }
    return OutputHelper.Return(message, 0);
  }
}
```

or `SynchronousCommand`

```csharp
public sealed class EchoCommand : SynchronousCommand {
  public override string Name => "echo";

  public override string Description => "Echoes the specified message.";

  public override string Usage => "echo <message>";

  public override int Execute(Arguments args) {
    if (!args.TryGetValue("message", out string message)) { // Validation
      // This example returns error code 400 (http bad request code) to signal client error
      // Any code you want can obviously be used
      Console.WriteLine("No message specified");
      return 404;
    }
    Console.WriteLine(message);
    return 0;
  }
}
```

As you can see the properties set the metadata for the command at compile time, and when it comes time to resolve it, no `reflection` is needed.

`ExecuteAsync` is returning a `ValueTask<int>` allowing both synchronous and asynchronous code, we use the high performance `Arguments` which is an object that manages arguments parsed from the input for retrieving and validating parameters. `Execute` is a synchronous alternative that just reduces the need of verbosity from `ValueTask.FromResult(int)` when `async` is not needed.

`OutputHelper.Return` is a helper method which outputs the message to customizable `TextWriter` in `CliRunner`, and returns the code (`int`) that is specified.

### Program.cs (Or other entry point)

```csharp
public static class Program {
  static ReadOnlySpan<Command> Commands => new Command[] {
    new EchoCommand(),
    new OtherCommand(), // This is for example sake, but can be anything
  };

  public static Task<int> Main(string[] args) {
    var runner = CliRunner.CreateBuilder()
        .AddCommands(Commands)
        .UseConsoleAsOutputWriter()
        .ModifyMetadata(metadata => {
       metadata.Name = "MyCli";
       metadata.Descriptions = "MyCli Description";
       metadata.Author = "John Doe";
       metadata.Version = "1.0.0";
       metadata.License = "MIT"
        })
        .Build();

   return runner.RunAsync(args).AsTask();
  }
}
```

We can add commands one by one, or use `params []` and `ReadOnlySpan<Command>`, if you want, you can also dynamically create an array of `Command`s from the executing assembly or any other using `reflection` and pass it as an argument, however this will be subject to trimming and can affect AOT compatibility.

Fluent API (builder pattern) is used to add the commands, set the output to the console (we can also set it to any `TextWriter`), and modify the global metadata that will be used for HelpText generation, and finally, build.

Running the app with `RunAsync` parses the `args`, and handles `help` requests, both global and per command, it delegates the execution to the appropriate command and injects arguments. After parsing the command name (first argument), `RunAsync` will also trigger `Arguments.ForwardPositionalArguments`, which will remove the command name and shift the arguments, so you don't need to account for it inside the logic of the command.

### Validation

Validation is performed at runtime depending on the actual logic inside the `ExecuteAsync` or `Execute` methods in each command. You choose how to interpret or handle each argument.

```csharp
public override int Execute(Arguments args) {
  if (!args.TryGetValue<int>("x", 20, out int x)) {
    // This examples checks if arguments has a named argument by name "x" (-x or --x)
    // And the value of this argument can be parsed as an integer.
    // A default value of 20 is also supplied
    // If the value is not found or can't be parsed, it will be set to the default value (20)
    // otherwise the parsed value.
    Console.WriteLine("X was not found or had an invalid format, setting it to default (20)");
  }
  Console.WriteLine(x);
  return 0;
}
```

Because you provide the actual type (no inference is needed), reflection is also not needed, thus, Native AOT compatibility is maintained without the possibility of trimming. With the consolidated APIs of `Arguments` you can validate and parse concisely with minimal verbosity.

### Minimalistic Structure Without Command Classes

As validation and parsing (the main pain points of CLI development) are manged through the `Arguments` object. You can use it directly if you don't need the global orchestration of `CliRunner`.

Example: Imagine you wanted a one `.cs` file that will take 2 numbers and add them, here's how to do that:

```csharp
using Sharpify.CommandLineInterface;
// using top level statements (>= .NET 5) Program.cs implicitly gets string[] args
Arguments arguments = Parser.ParseArguments(args);
// For the example we will decide that we expect named parameters x and y
if (!arguments.TryGetValue("x", 0, out int x)) {
  Console.WriteLine("Parameter \"x\" is required.");
  return 1; // 1 is a common code for error
}
if (!arguments.TryGetValue("y", 0, out int y)) {
  Console.WriteLine("Parameter \"y\" is required.");
  return 1;
}
// If we reached here, we validated and parsed x and y successfully
Console.WriteLine($"{x} + {y} = {x + y}");
return 0; // 0 is a common success code
```

In this example, we created a functional CLI that validates the existence and parses 2 named parameters, and used them, all in 10 lines of code.

### Arguments Key Logic

`Arguments` is a key-value-pair wrapper around `Dictionary<string, string>` which stores mapped arguments. To ensure a wide variety of applications, it parses arguments in the following way:

* Positional arguments are retrieved and parsed by using the position as key, for example: for the first argument (not named or flag), it could be retrieved by the key "0" or simply the number 0.
* Named arguments are parsed as regular key and value, dashes are removed from the key. So "--n" or "-n", key is "n". (But without dashes "n" will be registered as value of positional argument)
  * If a number is following a dash, it will be considered a numeric value, so don't use numbers as keys.
* Flags are like named arguments but whose value is empty, in order to avoid them being interpreted as named arguments, it is best practice to keep them *after* all the other parameters.

To handle the above there are the following overload resolutions in `Arguments`:

* `TryGetValue(int position, out string value)` - Will `.ToString()` the position and check the arguments.
* `TryGetValue(string key, out string value)` - Will check the arguments for the key.
* `HasFlag(string flag)` - Will check the arguments for the flag, so it will check both named key and that value is empty.
* `TryGetValue(ReadOnlySpan<string> keys, out string value)` - Will check the arguments for the keys, so the first matching key will be returned. This can be used to work with aliases (such as "-f" or "--file", it will find whichever the user enters and parse the value into the same variable)
* `ContainsKey(string key)` - Will check the arguments for the key. The argument in this case can be a named argument or flag, this overload doesn't distinguish between them.
* `ContainsKey(int position)` - Will `.ToString()` the position and check if a positional argument exists.

Arguments also support parameters which their value is a group of inputs, think of how the tool `rm` support any number of files, this is the same here. To further help working with these scenarios `TryGetValues` overloads accept a `separator` and return either a `string[]` or `T[]`. See all the options below.

### Arguments - All methods

```csharp
// CORE FUNCTIONS:
int Count;
bool AreEmpty;
static readonly Arguments Empty;
ReadOnlyMemory<string> ArgsAsMemory(); // inner input - after parsing and before mapping
ReadOnlySpan<string> ArgsAsSpan(); // same but as span
Arguments ForwardPositionalArguments(); // returns a new instance with the positional arguments forwarded
// So position 0 is deleted, and what was 1 becomes new 0, and so on.
// Non positional arguments are not affected.
ReadOnlyDictionary<string, string> GetInnerDictionary(); // returns the inner dictionary (advanced, useful mostly for debugging)

// SINGLE VALUE CHECKS:
bool Contains(string key);
bool Contains(int position);
bool HasFlag(string flag);
bool TryGetValue(int position, out string value);
bool TryGetValue(string key, out string value);
bool TryGetValue(ReadOnlySpan<string> keys, out string value);
/// T : IParsable<T>
bool TryGetValue<T>(int position, T defaultValue, out T value);
bool TryGetValue<T>(string key, T defaultValue, out T value);
bool TryGetValue<T>(ReadOnlySpan<string> keys, T defaultValue, out T value);
T GetValue<T>(string key, T defaultValue);
T GetValue<T>(int position, T defaultValue);
T GetValue<T>(ReadOnlySpan<string> keys, T defaultValue);
/// TEnum : struct, Enum
bool TryGetEnum<TEnum>(int position, out TEnum value);
bool TryGetEnum<TEnum>(int position, bool ignoreCase, out TEnum value);
bool TryGetEnum<TEnum>(int position, TEnum defaultValue, bool ignoreCase, out TEnum value);
bool TryGetEnum<TEnum>(string key, out TEnum value);
bool TryGetEnum<TEnum>(string key, bool ignoreCase, out TEnum value);
bool TryGetEnum<TEnum>(string key, TEnum defaultValue, bool ignoreCase, out TEnum value);
bool TryGetEnum<TEnum>(ReadOnlySpan<string> keys, out TEnum value);
bool TryGetEnum<TEnum>(ReadOnlySpan<string> keys, bool ignoreCase, out TEnum value);
bool TryGetEnum<TEnum>(ReadOnlySpan<string> keys, TEnum defaultValue, bool ignoreCase, out TEnum value);
TEnum GetEnum<TEnum>(int position, TEnum defaultValue);
TEnum GetEnum<TEnum>(int position, TEnum defaultValue, bool ignoreCase);
TEnum GetEnum<TEnum>(string key, TEnum defaultValue);
TEnum GetEnum<TEnum>(string key, TEnum defaultValue, bool ignoreCase);
TEnum GetEnum<TEnum>(ReadOnlySpan<string> keys, TEnum defaultValue);
TEnum GetEnum<TEnum>(ReadOnlySpan<string> keys, TEnum defaultValue, bool ignoreCase);

/// Multiple values (i.e. Arrays of values for single key)
bool TryGetValues(int position, string? separator, out string[] values);
bool TryGetValues(string key, string? separator, out string[] values);
bool TryGetValues(ReadOnlySpan<string> keys, string? separator, out string[] values);
/// T : IParsable<T> - Ensure to set the type for the out parameter
bool TryGetValues<T>(int position, string? separator, out T[] values);
bool TryGetValues<T>(string key, string? separator, out T[] values);
bool TryGetValues<T>(ReadOnlySpan<string> keys, string? separator, out T[] values);
```

### Custom Parsing

`Parser` is a static class that provides the functionality of mapping inputs to an `Arguments` object, it also has a function of parsing an input such as string (or `ReadOnlySpan<char>`) to a `List<string>`, it is efficient and different than `string.Split()` since it splits both on space and quotes, giving quotes priority, so that whatever is within quotes, will remain a single string, regardless of how many spaces there are inside. This can be especially important if you need file names that could contain spaces, or any other text.

`Parser` also has overloads for mapping arguments that configure a `StringComparer`, by default a `StringComparer.OrdinalIgnoreCase` is used, but whatever you prefer can be used instead.

### Overloads of `CliRunner.RunAsync`

`CliRunner.RunAsync` has overloads for `ReadOnlySpan<char>` (string), `IList<string>` (direct cast from `string[]` or `List<string>`), and `Arguments`, giving you full control over your input, and even custom parsing.

## Contact

For bug reports, feature requests or offers of support/sponsorship contact <dusrdev@gmail.com>

> This project is proudly made in Israel 🇮🇱 for the benefit of mankind.
