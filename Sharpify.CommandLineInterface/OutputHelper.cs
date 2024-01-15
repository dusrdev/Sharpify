namespace Sharpify.CommandLineInterface;

public static class OutputHelper {
    public static void WriteLine(string message) => CliRunner.OutputWriter.WriteLine(message);

    public static void Write(string message) => CliRunner.OutputWriter.Write(message);

    public static ValueTask<int> Return(string message, int code, bool appendCode = false) {
		var writer = CliRunner.OutputWriter;
		writer.Write(message);
		if (appendCode) {
			writer.Write($" [Code: {code}]");
		}
		writer.WriteLine();
		return ValueTask.FromResult(code);
	}
}