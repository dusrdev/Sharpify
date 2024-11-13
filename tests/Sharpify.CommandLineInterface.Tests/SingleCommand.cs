using System.Runtime.CompilerServices;

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

public sealed class SingleCommandNoParams : SynchronousCommand {
	public override string Name => "";

	public override string Description => "Changes the inner boxed value to true.";

	public override string Usage => "";

	private readonly StrongBox<bool> _value;

	public SingleCommandNoParams(StrongBox<bool> value) {
		_value = value;
	}

    public override int Execute(Arguments args) {
		_value.Value = true;
		return 0;
    }
}