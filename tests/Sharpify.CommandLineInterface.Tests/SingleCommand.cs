namespace Sharpify.CommandLineInterface.Tests;

public sealed class SingleCommand : Command {
	public override string Name => "";

	public override string Description => "Echoes the specified message.";

	public override string Usage => "Single <message>";

	public override ValueTask<int> ExecuteAsync(Arguments args) {
		if (!args.TryGetValue("message", out string message)) {
			return OutputHelper.Return("No message specified", 404, true);
		}
		return OutputHelper.Return(message, 0);
	}
}