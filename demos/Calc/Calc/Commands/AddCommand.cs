namespace Calc.Commands;

public class AddCommand : SynchronousCommand {
    public override string Name => "Add";

    public override string Description => "Add two numbers";

	public override string Usage => "Add <number1> <number2:optional>";

    public override int Execute(Arguments args) {
        if (!args.TryGetValue<int>(0, default(int), out var a)) {
			Console.WriteLine("Invalid number 1");
			return 1;
		}

		if (!args.TryGetValue<int>(1, 0, out var b)) {
			Console.WriteLine("Number 2 defaulted to 0");
			// return 1;
		}

		Console.WriteLine($"{a} + {b} = {a + b}");

		return 0;
    }
}