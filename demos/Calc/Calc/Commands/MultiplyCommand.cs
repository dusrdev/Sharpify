namespace Calc.Commands;

public class MultiplyCommand : SynchronousCommand {
    public override string Name => "Multiply";

    public override string Description => "Multiply two numbers";

    public override string Usage => "Multiply <number1> <number2>";

    public override int Execute(Arguments args) {
		if (!args.TryGetValue<double>(0, 0, out var a)) {
			Console.WriteLine("Invalid number 1");
			return 1;
		}

		if (!args.TryGetValue<double>(1, 0, out var b)) {
			Console.WriteLine("Invalid number 2");
			return 1;
		}

		Console.WriteLine($"{a} * {b} = {a * b}");

		return 0;
    }
}