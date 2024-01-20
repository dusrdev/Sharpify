using Sharpify.Collections;

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
		var length = Name.Length + Description.Length + Usage.Length + 40;
		if (length <= 1024) { // Small size is allocated on the stack
			var buffer = StringBuffer.Create(stackalloc char[length]);
			buffer.AppendLine();
			buffer.Append("Command: ");
			buffer.AppendLine(Name);
			buffer.AppendLine();
			buffer.Append("Description: ");
			buffer.AppendLine(Description);
			buffer.AppendLine();
			buffer.Append("Usage: ");
			buffer.AppendLine(Usage);
			return buffer.Allocate(true);
		} else { // Large size is rented from shared array pool
			using var buffer = StringBuffer.Rent(length);
			buffer.AppendLine();
			buffer.Append("Command: ");
			buffer.AppendLine(Name);
			buffer.AppendLine();
			buffer.Append("Description: ");
			buffer.AppendLine(Description);
			buffer.AppendLine();
			buffer.Append("Usage: ");
			buffer.AppendLine(Usage);
			return buffer.Allocate(true);
		}
	}
}