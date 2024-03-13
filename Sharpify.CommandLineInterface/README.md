# Sharpify.CommandLineInterface

`Sharpify.CommandLineInterface` is a high performance, reflection free and AOT-ready framework for creating command line interfaces, with a configurable output writer and no direct dependency to `System.Console` enabling it to be embedded, used with inputs from any source and output to any source.

Most other command line frameworks in c# use `reflection` to provide their "magic" such as generating help text, and providing input validation, `Sharpify.CommandLineInterface` instead uses compile time implemented metadata and static resolve of said metadata for this. each command, must implement the `Command` abstract class, part of which will be to set the command metadata, the main entry `CliRunner` also has an application level metadata object that can be customized in the `CliBuilder` process, using those, `Sharpify.CommandLineInterface` can resolve and format that metadata to generate an output similar to the other frameworks.

* `Sharpify.CommandLineInterface` although released starting at version `1.0.0`, should still be considered a beta, tools as flexible as this are very hard to test for edge cases.

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

`ExecuteAsync` is returning a `ValueTask<int>` allowing both synchronous and asynchronous code, we use the high performance `Arguments` which is an object that manages arguments parsed from the input, to retrieving and validating data. `Execute` is a sync alternative that just reduces the need of wrapping `ValueTask.FromResult(int)` verbosity when `async` is not needed.

`OutputHelper.Return` is a helper method which outputs the message to customizable `TextWriter` in `CliRunner`, and returns the code that is specified.

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

We can see that we can use high performances compiler optimized `ReadOnlySpan` to consolidate the commands,
We can also add command one by one, using `params []` or `ReadOnlySpan<Command>`, if you want, you can also dynamically create an array of `Command`s from the executing assembly or any other using `reflection` and pass it as an argument, however this won't be AOT-compatible.

Then we use the fluent api to add the commands, set the output to the console (we can also set it to any `TextWriter`), then we modify the global metadata and build.

Running the app with `RunAsync` parses the `args`, and handles `help` requests, both global and per command, delegates and forwards the arguments to the requested command by name, and executes.

### Notes

* Validation:
  * Argument validation per command should be done inside `ExecuteAsync` with the help of `Arguments`
  * You are trusted with the responsibility of not creating more than 1 `Command` with the same `Name`, failure to do so won't throw any exception, rather it will just execute on the first `Command` that matches the name from the `Arguments`
  * Any internal validation issue, will either be responded by returning `false` in any variant of `Arguments.TryGetValue`, or by outputting and returning error status code to the selected `TextWriter`, none cause critical failure, and this is on purpose to give the programmer maximum control on handling issues.
* `Arguments` also supports **positional** parameters, and they are parsed such that their key is their position, so position 0 is at key "0", you can get the parameter either by `TryGetValue("0"...)` or using the `int` overload.
* In order to make it more convenient, it is possible to advance the positional parameters so that each `Command` perhaps could be developed without thinking about the previous possible positional arguments, this is done using `Arguments.ForwardPositionalArguments()` which will return a new instance with the parameters advanced. This happens already inside `CliRunner.RunAsync` when after it delegates the arguments to a `Command` so that the positional argument 0 (which is the command name) is removed.
* `Arguments` also has overloads to get any `IParsable<T>` value, or `enum`, both of those overloads require a default value, that will be returned if the `key` wasn't found or value unable to be parsed.
* `Arguments` also handles optional `bool` type parameters that just require specifying to be regarded as true, it does this by creating a key for them, with the value set to `""`, using `Arguments.TryGetValue` to get these types of parameters will give confusing results as the `output` will be set to `""` if the key doesn't exist anyway. To get accurate results, you can use `Arguments.Contains(key)`.
* `Parser` is a static class that provides the functionality of parsing inputs to `Arguments`, it also has a function of parsing an input such as string (or `ReadOnlySpan<char>`) to a `List<string>`, it is efficient and different than `string.Split()` since it splits both on space and quotes, giving quotes priority, so that whatever is within quotes, will remain a single string, regardless of how many spaces there are inside. This can be especially important if you need perhaps file names that could contain spaces, or any other text.
* `Parser` also has overloads for parsing arguments that configure a `StringComparer`, by default a `CurrentCultureIgnoreCase` is used, but whatever you prefer can be used instead.
* `CliRunner.RunAsync` has overloads for `ReadOnlySpan<char>` (string), `ReadOnlySpan<string>` (array), and `Arguments` giving you full control over your input, and even custom parsing.
* `Command` help text is resolves using the virtual `GetHelp` method, it can be overridden to suit you needs, you can use `CliRunner.StrBuilder` instance to modify anything virtually allocation free (just remember to clear the `StringBuilder` first)
* The only place an exception can be thrown is when a `CliRunner` is trying to be created without any commands loaded in the `CliBuilder`, this should not "surprise" anyone as it should never happen after design time.
