namespace Sharpify.CommandLineInterface;

/// <summary>
/// Represents a command for a CLI application.
/// </summary>
public abstract class Command {
	/// <summary>
	/// Gets the name of the command.
	/// </summary>
	public abstract string Name { get; }
	/// <summary>
	/// Gets the description of the command.
	/// </summary>
	public abstract string Description { get; }
	/// <summary>
	/// Gets the usage of the command.
	/// </summary>
	public abstract string Usage { get; }

	/// <summary>
	/// Executes the command.
	/// </summary>
	public abstract ValueTask<int> ExecuteAsync(Arguments args);

	/// <summary>
	/// Gets the help for the command.
	/// </summary>
	public virtual string GetHelp() {
		var builder = CliRunner.StrBuilder.Value!;
		builder.Clear()
			.AppendLine()
            .Append("Command: ")
            .AppendLine(Name)
            .AppendLine()
            .Append("Description: ")
            .AppendLine(Description)
            .AppendLine()
            .Append("Usage: ")
            .AppendLine(Usage);
		return builder.ToString();
	}
}