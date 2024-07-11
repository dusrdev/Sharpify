namespace Sharpify.CommandLineInterface;

/// <summary>
/// Represents a builder for a CliRunner.
/// </summary>
public sealed class CliBuilder {
	private readonly List<Command> _commands;

	private readonly CliMetadata _metaData;

	private CliRunnerOptions _options;
	private string _header = "";

	internal CliBuilder() {
		_commands = new List<Command>();
		_metaData = CliMetadata.Default;
	}

	/// <summary>
	/// Adds a command to the CLI runner.
	/// </summary>
	/// <param name="command"></param>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	public CliBuilder AddCommand(Command command) {
		_commands.Add(command);
		return this;
	}

	/// <summary>
	/// Adds commands to the CLI runner.
	/// </summary>
	/// <param name="commands"></param>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	public CliBuilder AddCommands(params Command[] commands) {
		_commands.AddRange(commands);
		return this;
	}

	/// <summary>
	/// Adds commands to the CLI runner.
	/// </summary>
	/// <param name="commands"></param>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	public CliBuilder AddCommands(ReadOnlySpan<Command> commands) {
		_commands.AddRange(commands);
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
		_options |= CliRunnerOptions.SortCommandsAlphabetically;
		return this;
	}

	/// <summary>
	/// Use metadata in the help text of the CLI runner.
	/// </summary>
	/// <remarks>
	/// Has priority over the custom header, and only one is used, so including a custom header as well will not do anything.
	/// </remarks>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	public CliBuilder WithMetadata(Action<CliMetadata> action) {
		action(_metaData);
		_options |= CliRunnerOptions.IncludeMetadata;
		return this;
	}

	/// <summary>
	/// Use a custom header instead of Metadata in the header of the help text
	/// </summary>
	/// <remarks>
	/// <see cref="CliMetadata"/> as priority over the custom header, and only one is used, so make sure not to include it if you want the custom header to show.
	/// </remarks>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	public CliBuilder WithCustomHeader(string header) {
		_header = header;
		_options |= CliRunnerOptions.UseCustomHeader;
		return this;
	}

	/// <summary>
	/// Builds the CLI runner.
	/// </summary>
	public CliRunner Build() {
		if (_commands.Count is 0) {
			throw new InvalidOperationException("No commands were added.");
		}
		return new CliRunner(_commands, _options, _metaData, _header);
	}
}