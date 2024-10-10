using System.Buffers;
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

	/// <summary>
	/// Gets the commands registered with the CLI runner.
	/// </summary>
	public ReadOnlyCollection<Command> Commands => _config.Commands.AsReadOnly();

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

	private readonly CliRunnerConfiguration _config;

	/// <summary>
	/// Creates a new instance of the <see cref="CliRunner"/> class.
	/// </summary>
	/// <remarks>To be used with the <see cref="CliBuilder"/></remarks>
	internal CliRunner(CliRunnerConfiguration config) {
		_config = config;
		// If there is only one command, sorting is not necessary
		if (_config.SortCommandsAlphabetically && _config.Commands.Count is not 1) {
			_config.Commands.Sort(Command.ByNameComparer);
		}
	}

	/// <summary>
	/// Handles the case where no input was provided.
	/// </summary>
	/// <returns></returns>
	private ValueTask<int> HandleNoInput(bool commandNameRequired) =>
			_config.OutputHelpTextForEmptyInput
			? OutputHelper.Return(GenerateHelpText(commandNameRequired), 0)
			: OutputHelper.Return("No command specified", 404, _config.ShowErrorCodes);

	/// <summary>
	/// Runs the CLI application with the specified arguments.
	/// </summary>
	public ValueTask<int> RunAsync(ReadOnlySpan<char> args, bool commandNameRequired = true) {
		if (args.Length is 0) {
			return HandleNoInput(commandNameRequired);
		}
		var arguments = Parser.ParseArguments(args, _config.GetComparer());
		return RunAsync(arguments, commandNameRequired);
	}

	/// <summary>
	/// Runs the CLI application with the specified arguments.
	/// </summary>
	public ValueTask<int> RunAsync(ReadOnlySpan<string> args, bool commandNameRequired = true) {
		if (args.Length is 0) {
			return HandleNoInput(commandNameRequired);
		}
		var arguments = Parser.ParseArguments(args, _config.GetComparer());
		return RunAsync(arguments, commandNameRequired);
	}

	/// <summary>
	/// Runs the CLI application with the specified arguments.
	/// </summary>
	public ValueTask<int> RunAsync(Arguments? arguments, bool commandNameRequired = true) {
		if (arguments is null) {
			return OutputHelper.Return("Input could not be parsed", 400, _config.ShowErrorCodes);
		}

		string version = $"Version: {_config.MetaData.Version}"; // cache version

		// general help text
		if (arguments.IsFirstOrFlag("help")) {
			return OutputHelper.Return(GenerateHelpText(commandNameRequired), 0);
		}
		if (arguments.IsFirstOrFlag("version")) {
			return OutputHelper.Return(version, 0);
		}

		// Only for single command CLIs
		if (!commandNameRequired) {
			// If there is more than one command, the command name is required
			if (_config.Commands.Count is not 1) {
				return OutputHelper.Return("Command name is required when using more than one command", 405, _config.ShowErrorCodes);
			}
			// Execute the command
			return _config.Commands[0].ExecuteAsync(arguments);
		}

		if (!arguments.TryGetValue(0, out string commandName)) {
			return OutputHelper.Return("Command name is required", 405, _config.ShowErrorCodes);
		}

		Command? command = _config.Commands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
		if (command == default) {
			return OutputHelper.Return($"Command \"{commandName}\" not found.", 404, _config.ShowErrorCodes);
		}

		if (arguments.Contains("help") || arguments.HasFlag("help")) {
			return OutputHelper.Return(command.GetHelp(), 0);
		}
		return command.ExecuteAsync(arguments.ForwardPositionalArguments());
	}

	// Generates the help for the application - happens once, at initialization of CliRunner
	private string GenerateHelpText(bool commandNameRequired) {
		// here the likely help text is larger than per command, so we use a rented buffer
		using var owner = MemoryPool<char>.Shared.Rent(GetRequiredBufferLength());
		var buffer = StringBuffer.Create(owner.Memory.Span);
		buffer.AppendLine();
		if (_config.IncludeMetadata) {
			var metaData = _config.MetaData;
			buffer.AppendLine(metaData.Name);
			buffer.AppendLine();
			buffer.AppendLine(metaData.Description);
			buffer.AppendLine();
			buffer.Append("Author: ");
			buffer.AppendLine(metaData.Author);
			buffer.Append("Version: ");
			buffer.AppendLine(metaData.Version);
			buffer.Append("License: ");
			buffer.AppendLine(metaData.License);
			buffer.AppendLine();
		} else if (_config.UseCustomHeader) {
			buffer.AppendLine(_config.Header);
			buffer.AppendLine();
		}
		if (commandNameRequired) {
			buffer.AppendLine("Commands:");
			var maxCommandLength = GetMaximumCommandLength() + 2;
			foreach (Command command in _config.Commands) {
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
		} else {
			var command = _config.Commands[0];
			buffer.Append("Usage: ");
			buffer.AppendLine(command.Usage);
		}

		return buffer.Allocate();
	}

	private int GetMaximumCommandLength() => _config.Commands.Max(c => c.Name.Length);

	private int GetRequiredBufferLength() {
		int length = (_config.Commands.Count + 5) * 256; // default buffer for commands and possible extra text
		if (_config.IncludeMetadata) {
			length += _config.MetaData.TotalLength;
		} else if (_config.UseCustomHeader) {
			length += _config.Header.Length;
		}
		return length;
	}
}