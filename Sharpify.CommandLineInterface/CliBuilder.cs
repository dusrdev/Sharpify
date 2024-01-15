using System.Reflection;

namespace Sharpify.CommandLineInterface;

/// <summary>
/// Represents a builder for a CLI application.
/// </summary>
public sealed class CliBuilder {
	private readonly List<Command> _commands;

	private readonly CliMetadata _metaData;

	internal CliBuilder() {
		_commands = new();
		_metaData = CliMetadata.Default;
	}

/// <inheritdoc/>
	public CliBuilder AddCommand(Command command) {
		_commands.Add(command);
		return this;
	}

/// <inheritdoc/>
	public CliBuilder AddCommands(params Command[] commands) {
		_commands.AddRange(commands);
		return this;
	}

/// <inheritdoc/>
	public CliBuilder AddCommands(ReadOnlySpan<Command> commands) {
		_commands.AddRange(commands);
		return this;
	}

/// <inheritdoc/>
	public CliBuilder AddCommandsFromAssembly(Assembly assembly) {
		foreach (Type type in assembly.GetTypes()) {
			if (type.IsAssignableTo(typeof(Command))) {
				_commands.Add((Command)Activator.CreateInstance(type)!);
			}
		}
		return this;
	}

/// <inheritdoc/>
	public CliBuilder AddCommandsFromExecutingAssembly() {
		return AddCommandsFromAssembly(Assembly.GetExecutingAssembly());
	}

/// <inheritdoc/>
	public CliBuilder SetOutputWriter(TextWriter writer) {
		CliRunner.SetOutputWriter(writer);
		return this;
	}

/// <inheritdoc/>
	public CliBuilder UseConsoleAsOutputWriter() {
		CliRunner.SetOutputWriter(Console.Out);
		return this;
	}

/// <inheritdoc/>
	public CliBuilder ModifyMetadata(Action<CliMetadata> action) {
		action(_metaData);
		return this;
	}

/// <inheritdoc/>
	public CliRunner Build() {
		if (_commands.Count is 0) {
			throw new InvalidOperationException("No commands were added.");
		}
		return new CliRunner(_commands, _metaData);
	}
}