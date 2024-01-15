namespace Sharpify.CommandLineInterface.Tests;

public sealed class AddCommand : Command {
	public override string Name => "add";

	public override string Description => "Adds 2 numbers.";

	public override string Usage => "add <number1> <number2>";

	public override ValueTask<int> ExecuteAsync(Arguments args) {
		if (!args.TryGetValue(0, 0, out var number1)) {
			return OutputHelper.Return("<number1> not specified", 404, true);
		}
		if (!args.TryGetValue(1, 0, out var number2)) {
			return OutputHelper.Return("<number2> not specified", 404, true);
		}
		return OutputHelper.Return($"{number1} + {number2} = {number1 + number2}", 0);
	}
}