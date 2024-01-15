namespace Sharpify.CommandLineInterface;

/// <summary>
/// Represents a builder for a CliRunner.
/// </summary>
public sealed class CliBuilder {
	private readonly List<Command> _commands;

	private readonly CliMetadata _metaData;

	internal CliBuilder() {
		_commands = new();
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
	/// Modifies the metadata for the CLI runner, this is used in the general help text
	/// </summary>
	/// <returns>The same instance of <see cref="CliBuilder"/></returns>
	public CliBuilder ModifyMetadata(Action<CliMetadata> action) {
		action(_metaData);
		return this;
	}

	/// <summary>
	/// Builds the CLI runner.
	/// </summary>
	public CliRunner Build() {
		if (_commands.Count is 0) {
			throw new InvalidOperationException("No commands were added.");
		}
		return new CliRunner(_commands, _metaData);
	}
}