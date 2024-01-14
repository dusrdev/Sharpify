namespace Sharpify.Cli;

public abstract class Command {
	public abstract string Name { get; }
	public abstract string Description { get; }
	public abstract string Usage { get; }

	public abstract Task<int> ExecuteAsync(string[] args);
}