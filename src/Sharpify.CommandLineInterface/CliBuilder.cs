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
	/// Configures how the parser handles argument casing.
	/// </summary>
	/// <remarks>
	/// By default it is set to <see cref="ArgumentCaseHandling.IgnoreCase"/> to improve user experience
	/// </remarks>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	public CliBuilder ConfigureArgumentCaseHandling(ArgumentCaseHandling caseHandling) {
		_config.ArgumentCaseHandling = caseHandling;
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
	/// Configures how the CLI runner handles empty input.
	/// </summary>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	public CliBuilder ConfigureEmptyInputBehavior(EmptyInputBehavior behavior) {
		_config.EmptyInputBehavior = behavior;
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