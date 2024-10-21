namespace Sharpify.CommandLineInterface;

/// <summary>
/// Represents a builder for a CliRunner.
/// </summary>
public sealed class CliBuilder {
	private readonly CliRunnerConfiguration _config;

	internal CliBuilder() {
		_config = new CliRunnerConfiguration();
	}

	/// <summary>
	/// Adds a command to the CLI runner.
	/// </summary>
	/// <param name="command"></param>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	public CliBuilder AddCommand(Command command) {
		_config.Commands.Add(command);
		return this;
	}

	/// <summary>
	/// Adds commands to the CLI runner.
	/// </summary>
	/// <param name="commands"></param>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	public CliBuilder AddCommands(ReadOnlySpan<Command> commands) {
		_config.Commands.AddRange(commands);
		return this;
	}

	/// <summary>
	/// Sets the output writer for the CLI runner.
	/// </summary>
	/// <param name="writer"></param>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	public CliBuilder SetOutputWriter(TextWriter writer) {
		CliRunner.SetOutputWriter(writer);
		return this;
	}

	/// <summary>
	/// Sets the output writer for the CLI runner to be <see cref="Console.Out"/>.
	/// </summary>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	public CliBuilder UseConsoleAsOutputWriter() {
		CliRunner.SetOutputWriter(Console.Out);
		return this;
	}

	/// <summary>
	/// Sorts the commands alphabetically.
	/// </summary>
	/// <remarks>
	/// This change only affects the functionality of the help text.
	/// </remarks>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	public CliBuilder SortCommandsAlphabetically() {
		_config.SortCommandsAlphabetically = true;
		return this;
	}

	/// <summary>
	/// Add metadata - can be used to generate the general help text (Is the default source)
	/// </summary>
	/// <remarks>
	/// Configure the help text source with <see cref="SetHelpTextSource(HelpTextSource)"/>
	/// </remarks>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	public CliBuilder WithMetadata(Action<CliMetadata> options) {
		options(_config.MetaData);
		return this;
	}

	/// <summary>
	/// Add a custom header - can be used instead of Metadata in the header of the help text
	/// </summary>
	/// <remarks>
	/// Configure the help text source with <see cref="SetHelpTextSource(HelpTextSource)"/>
	/// </remarks>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	public CliBuilder WithCustomHeader(string header) {
		_config.CustomHeader = header;
		return this;
	}

	/// <summary>
	/// Sets the source of the general help text.
	/// </summary>
	/// <param name="source">Requested source of the help text.</param>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	public CliBuilder SetHelpTextSource(HelpTextSource source) {
		_config.HelpTextSource = source;
		return this;
	}

	/// <summary>
	/// Configures the arguments to be case sensitive.
	/// </summary>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	/// <remarks>
	/// This can be useful if you have many short flags, like grep
	/// </remarks>
	public CliBuilder WithCaseSensitiveParameters() {
		_config.IgnoreParameterCase = false;
		return this;
	}

	/// <summary>
	/// Show error codes next to the error <see cref="CliRunner"/> messages.
	/// </summary>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	public CliBuilder ShowErrorCodes() {
		_config.ShowErrorCodes = true;
		return this;
	}

	/// <summary>
	/// Disables automatic defaulting to help text when no input is provided. Instead an error indicating that no command was found will be shown.
	/// </summary>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	public CliBuilder WithoutHelpTextForEmptyInput() {
		_config.OutputHelpTextForEmptyInput = false;
		return this;
	}

	/// <summary>
	/// Builds the CLI runner.
	/// </summary>
	public CliRunner Build() {
		if (_config.Commands.Count is 0) {
			throw new InvalidOperationException("No commands were added.");
		}
		return new CliRunner(_config);
	}
}