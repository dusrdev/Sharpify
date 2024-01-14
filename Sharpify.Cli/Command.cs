namespace Sharpify.Cli;

public abstract class Command {
	public abstract string Name { get; }
	public abstract string Description { get; }
	public abstract string Usage { get; }
	public abstract ReadOnlySpan<Parameter> Parameters { get; }

	public abstract ValueTask<int> ExecuteAsync(ReadOnlySpan<char> args);

	public virtual void PrintUsage(TextWriter writer) {
		writer.WriteLine(Name);
		writer.WriteLine();
		writer.WriteLine(Description);
		writer.WriteLine();
		writer.WriteLine("Usage:");
		writer.WriteLine(Usage);
		writer.WriteLine();
	}
}