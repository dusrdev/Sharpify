namespace Sharpify.CommandLineInterface;

/// <summary>
/// An alternative to <see cref="Command"/> that runs synchronously.
/// </summary>
/// <remarks>
/// This is syntactic sugar for wrapping returns from ExecuteAsync in ValueTask.FromResult
/// </remarks>
public abstract class SynchronousCommand : Command {
	/// <inheritdoc/>
	public override ValueTask<int> ExecuteAsync(Arguments args) {
		return ValueTask.FromResult(Execute(args));
	}

	/// <summary>
	/// Executes the command.
	/// </summary>
	/// <param name="args"></param>
	/// <returns>Status code</returns>
	public abstract int Execute(Arguments args);
}