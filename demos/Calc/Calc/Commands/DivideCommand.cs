namespace Calc.Commands;

public class DivideCommand : SynchronousCommand {
    public override string Name => "Divide";

    public override string Description => "Divide one number by another";

    public override string Usage => "Divide <number1> <number2>";

    public override int Execute(Arguments args) {
		if (!args.TryGetValue<double>(0, 0, out var a)) {
			Console.WriteLine("Invalid number 1");
			return 1;
		}

		if (!args.TryGetValue<double>(1, 0, out var b) || b == 0) {
			Console.WriteLine("Invalid number 2");
			return 1;
		}

		Console.WriteLine($"{a} / {b} = {a / b}");

		return 0;
    }
}