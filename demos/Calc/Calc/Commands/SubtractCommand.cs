namespace Calc.Commands;

public class SubtractCommand : SynchronousCommand {
    public override string Name => "Subtract";

    public override string Description => "Subtract one number from another";

    public override string Usage =>
	"""
	Subtract <number1> <number2> [options]
		Options:
			--hex - Display the result in hexadecimal";
	""";

    public override int Execute(Arguments args) {
		if (!args.TryGetValue<int>(0, 0, out int a)) {
			Console.WriteLine("Invalid number 1");
			return 1;
		}

		if (!args.TryGetValue<int>(1, 0, out int b)) {
			Console.WriteLine("Invalid number 2");
			return 1;
		}

		if (args.HasFlag("hex")) {
			Console.WriteLine($"{a} - {b} = {a - b:X}");
			return 0;
		} else {
			Console.WriteLine($"{a} - {b} = {a - b}");
		}

		return 0;
    }
}