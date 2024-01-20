using System.Collections.ObjectModel;

using Sharpify.Collections;

namespace Sharpify.CommandLineInterface;

/// <summary>
/// Provides the means of running a CLI and configuring package wide settings.
/// </summary>
public sealed class CliRunner {
	/// <summary>
	/// Creates a new instance of the <see cref="CliBuilder"/> class.
	/// </summary>
	public static CliBuilder CreateBuilder() => new();

	private readonly List<Command> _commands;

	/// <summary>
	/// Gets the commands registered with the CLI runner.
	/// </summary>
	public ReadOnlyCollection<Command> Commands => _commands.AsReadOnly();

	/// <summary>
	/// Gets the output writer for the CLI runner.
	/// </summary>
	/// <remarks>Defaults to <see cref="TextWriter.Null"/></remarks>
	public static TextWriter OutputWriter { get; private set; } = TextWriter.Null;

	/// <summary>
	/// Sets the output writer for the CLI runner.
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
			if (_commands.Count is not 1) {
				return OutputHelper.Return("Command name is required when using more than one command", 405, true);
			}
			if (arguments.Contains("help")) {
				return OutputHelper.Return(_commands[0].GetHelp(), 0);
			}
			return _commands[0].ExecuteAsync(arguments);
		}

		if (arguments.Count is 1 && arguments.Contains("help")) {
			return OutputHelper.Return(_help, 0);
		}

		if (!arguments.TryGetValue(0, out string commandName)) {
			return OutputHelper.Return("Command name is required", 405, true);
		}

		if (commandName.Equals("help", StringComparison.OrdinalIgnoreCase)) {
			return OutputHelper.Return(_help, 0);
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
		var length = _metaData.IncludeInHelpText ?
			_metaData.TotalLength + _commands.Count * 128 + 256
			: _commands.Count * 128 + 256;
		// here the likely help text is larger than per command, so we use a rented buffer
		var buffer = StringBuffer.Rent(length);
		buffer.AppendLine();
		if (_metaData.IncludeInHelpText) {
			buffer.AppendLine(_metaData.Name);
			buffer.AppendLine();
			buffer.AppendLine(_metaData.Description);
			buffer.AppendLine();
			buffer.Append("Author: ");
			buffer.AppendLine(_metaData.Author);
			buffer.Append("Version: ");
			buffer.AppendLine(_metaData.Version);
			buffer.Append("License: ");
			buffer.AppendLine(_metaData.License);
			buffer.AppendLine();
		}
		buffer.AppendLine("Commands:");
		var maxCommandLength = GetMaximumCommandLength();
		foreach (Command command in _commands.AsSpan()) {
			buffer.Append(command.Name.PadRight(maxCommandLength));
			buffer.Append(" - ");
			buffer.AppendLine(command.Description);
		}
		buffer.Append(
			"""

			To get help for a command, use the following syntax:
			<command> --help

			To get help for the application, use the following syntax:
			--help

			"""
		);

		return buffer.Allocate(true);
	}

	private int GetMaximumCommandLength() {
		int max = 0;
		foreach (Command command in _commands.AsSpan()) {
			if (command.Name.Length > max) {
				max = command.Name.Length;
			}
		}
		return max;
	}
}