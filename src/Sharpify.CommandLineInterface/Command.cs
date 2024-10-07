using System.Buffers;

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
		var length = (Name.Length + Description.Length + Usage.Length) * 2;
		using var owner = MemoryPool<char>.Shared.Rent(length);
		var buffer = StringBuffer.Create(owner.Memory.Span);
		buffer.AppendLine();
		buffer.Append("Command: ");
		buffer.AppendLine(Name);
		buffer.AppendLine();
		buffer.Append("Description: ");
		buffer.AppendLine(Description);
		buffer.AppendLine();
		buffer.Append("Usage: ");
		buffer.AppendLine(Usage);
		return buffer.Allocate();
	}

	/// <summary>
	/// Compares two commands by their name.
	/// </summary>
	/// <param name="x">The first command to compare.</param>
	/// <param name="y">The second command to compare.</param>
	/// <returns>
	/// A value indicating the relative order of the commands.
	/// The return value is less than 0 if x.Name is less than y.Name,
	/// 0 if x.Name is equal to y.Name, and greater than 0 if x.Name is greater than y.Name.
	/// </returns>
	public static int ByNameComparer(Command x, Command y) {
		return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
	}
}