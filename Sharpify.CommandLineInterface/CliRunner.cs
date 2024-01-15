using System.Text;

namespace Sharpify.CommandLineInterface;

/// <summary>
/// Provides the means of running a CLI application and configuring package wide settings.
/// </summary>
public sealed class CliRunner {
	/// <summary>
	/// Creates a new instance of the <see cref="CliBuilder"/> class.
	/// </summary>
	public static CliBuilder CreateBuilder() => new();

	/// <summary>
	/// A thread local <see cref="StringBuilder"/> instance that can be used to generate outputs while minimizing allocations.
	/// </summary>
	public static readonly ThreadLocal<StringBuilder> StrBuilder = new(() => new StringBuilder());

	private readonly List<Command> _commands;

	/// <summary>
	/// Gets the output writer for the CLI application.
	/// </summary>
	/// <remarks>Defaults to <see cref="TextWriter.Null"/></remarks>
	public static TextWriter OutputWriter { get; private set; } = TextWriter.Null;

	/// <summary>
	/// Sets the output writer for the CLI application.
	/// </summary>
	public static void SetOutputWriter(TextWriter writer) {
		OutputWriter = writer;
	}

	private readonly CliMetadata _metaData;
	private readonly string _help;

	/// <summary>
	/// Creates a new instance of the <see cref="CliRunner"/> class.
	/// </summary>
	/// <remarks>To be used with the <see cref="CliBuilder"/></remarks>
    internal CliRunner(List<Command> commands, CliMetadata metaData) {
        _commands = commands;
        _metaData = metaData;
		_help = GenerateHelp();
    }

	/// <summary>
	/// Runs the CLI application with the specified arguments.
	/// </summary>
    public ValueTask<int> RunAsync(ReadOnlySpan<char> args, bool commandNameRequired = true) {
		if (args.Length is 0) {
			return OutputHelper.Return("No command specified", 404, true);
		}
		var arguments = Parser.ParseArguments(args);
		return RunAsync(arguments, commandNameRequired);
	}

	/// <summary>
	/// Runs the CLI application with the specified arguments.
	/// </summary>
	public ValueTask<int> RunAsync(ReadOnlySpan<string> args, bool commandNameRequired = true) {
		if (args.Length is 0) {
			return OutputHelper.Return("No command specified", 404, true);
		}
		var arguments = Parser.ParseArguments(args, StringComparer.CurrentCultureIgnoreCase);
		return RunAsync(arguments, commandNameRequired);
	}

	/// <summary>
	/// Runs the CLI application with the specified arguments.
	/// </summary>
	public ValueTask<int> RunAsync(Arguments? arguments, bool commandNameRequired = true) {
		if (arguments is null) {
			return OutputHelper.Return("Input could not be parsed", 400, true);
		}
		if (!commandNameRequired) {
			if (_commands.Count is 1) {
				if (arguments.Contains("help")) {
					OutputWriter.WriteLine(_commands[0].GetHelp());
				}
				return _commands[0].ExecuteAsync(arguments);
			}
			return OutputHelper.Return("Command name is required when using more than one command", 405, true);
		}
		if (arguments.Count is 1 && arguments.Contains("help")) {
			OutputWriter.WriteLine(_help);
			return ValueTask.FromResult(0);
		}

		if (!arguments.TryGetValue(0, out string commandName)) {
			return OutputHelper.Return("Command name is required", 405, true);
		}

		Command? command = null;
		foreach (Command c in _commands.AsSpan()) {
			if (c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase)) {
				command = c;
				break;
			}
		}
		if (command is null) {
			return OutputHelper.Return($"Command \"{commandName}\" not found.", 404, true);
		}
		if (arguments.Contains("help")) {
			OutputWriter.WriteLine(command.GetHelp());
			return ValueTask.FromResult(0);
		}
		return command.ExecuteAsync(arguments.ForwardPositionalArguments());
	}

	// Generates the help for the application - happens once, at initialization of CliRunner
	private string GenerateHelp() {
		var builder = StrBuilder.Value!;
		builder.Clear()
			.AppendLine()
			.AppendLine(_metaData.Name)
			.AppendLine()
			.AppendLine(_metaData.Description)
			.AppendLine()
			.Append("Author: ")
			.AppendLine(_metaData.Author)
			.Append("Version: ")
			.AppendLine(_metaData.Version)
			.Append("License: ")
			.AppendLine(_metaData.License)
			.AppendLine()
			.AppendLine("Commands:");
		var maxCommandLength = _commands.Max(static c => c.Name.Length);
		foreach (Command command in _commands.AsSpan()) {
			builder.Append(command.Name.PadRight(maxCommandLength))
                .Append(" - ")
                .AppendLine(command.Description);
		}
		builder
			.AppendLine()
			.AppendLine("To get help for a command, use the following syntax:")
			.AppendLine("<command> --help")
			.AppendLine()
			.AppendLine("To get help for the application, use the following syntax:")
			.AppendLine("--help")
			.AppendLine();
		return builder.ToString();
	}
}