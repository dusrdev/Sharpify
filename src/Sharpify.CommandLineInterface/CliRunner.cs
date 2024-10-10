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
	private readonly bool _isSingleCommand;

	/// <summary>
	/// Creates a new instance of the <see cref="CliRunner"/> class.
	/// </summary>
	/// <remarks>To be used with the <see cref="CliBuilder"/></remarks>
	internal CliRunner(CliRunnerConfiguration config) {
		_config = config;
		_isSingleCommand = _config.Commands.Count is 1;
		// If there is only one command, sorting is not necessary
		if (_config.SortCommandsAlphabetically && !_isSingleCommand) {
			_config.Commands.Sort(Command.ByNameComparer);
		}
	}

	/// <summary>
	/// Handles the case where no input was provided.
	/// </summary>
	/// <returns></returns>
	private ValueTask<int> HandleNoInput() =>
			_config.OutputHelpTextForEmptyInput
			? OutputHelper.Return(GenerateHelpText(isSingleCommand: _isSingleCommand), 0)
			: OutputHelper.Return("No command specified", 404, _config.ShowErrorCodes);

	/// <summary>
	/// Runs the CLI application with the specified arguments.
	/// </summary>
	public ValueTask<int> RunAsync(ReadOnlySpan<char> args, bool commandNameRequired = true) {
		if (args.Length is 0) {
			return HandleNoInput();
		}
		var arguments = Parser.ParseArguments(args, _config.GetComparer());
		return RunAsync(arguments, commandNameRequired);
	}

	/// <summary>
	/// Runs the CLI application with the specified arguments.
	/// </summary>
	public ValueTask<int> RunAsync(ReadOnlySpan<string> args, bool commandNameRequired = true) {
		if (args.Length is 0) {
			return HandleNoInput();
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

		// Only for single command CLIs
		if (!commandNameRequired) {
			// If there is more than one command, the command name is required
			if (!_isSingleCommand) {
				return OutputHelper.Return("Command name is required when using more than one command", 405, _config.ShowErrorCodes);
			}
			// Special kind of help text for single command CLIs
			if (arguments.Contains("help") || arguments.HasFlag("help")) {
				return OutputHelper.Return(GenerateHelpText(isSingleCommand: _isSingleCommand), 0);
			} else if (arguments.Contains("version") || arguments.HasFlag("version")) {
				return OutputHelper.Return($"Version: {_config.MetaData.Version}", 0);
			} else {
				// Execute the command
				return _config.Commands[0].ExecuteAsync(arguments);
			}
		}

		if (!arguments.TryGetValue(0, out string commandName)) {
			if (arguments.HasFlag("help")) {
				return OutputHelper.Return(GenerateHelpText(isSingleCommand: _isSingleCommand), 0);
			} else if (arguments.HasFlag("version")) {
				return OutputHelper.Return($"Version: {_config.MetaData.Version}", 0);
			} else {
				return OutputHelper.Return("Command name is required", 405, _config.ShowErrorCodes);
			}
		}

		if (commandName.Equals("help", StringComparison.OrdinalIgnoreCase)) {
			return OutputHelper.Return(GenerateHelpText(isSingleCommand: _isSingleCommand), 0);
		} else if (commandName.Equals("version", StringComparison.OrdinalIgnoreCase)) {
			return OutputHelper.Return($"Version: {_config.MetaData.Version}", 0);
		}

		Command? command = _config.Commands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
		if (command == default) {
			return OutputHelper.Return($"Command \"{commandName}\" not found.", 404, _config.ShowErrorCodes);
		}

		if (arguments.Contains("help") || arguments.HasFlag("help")) {
			return OutputHelper.Return(command.GetHelp(), 0);
		} else {
			return command.ExecuteAsync(arguments.ForwardPositionalArguments());
		}
	}

	// Generates the help for the application - happens once, at initialization of CliRunner
	private string GenerateHelpText(bool isSingleCommand) {
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
		if (isSingleCommand) {
			var command = _config.Commands[0];
			buffer.AppendLine("Usage:");
			buffer.AppendLine(command.Usage);
		} else {
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