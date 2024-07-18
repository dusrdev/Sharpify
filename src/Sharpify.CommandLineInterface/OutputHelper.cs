namespace Sharpify.CommandLineInterface;

/// <summary>
/// Provides helper methods for outputting using <see cref="CliRunner.OutputWriter"/>
/// </summary>
public static class OutputHelper {
	/// <summary>
	/// Writes a line to the output writer.
	/// </summary>
    public static void WriteLine(string message) => CliRunner.OutputWriter.WriteLine(message);

	/// <summary>
	/// Writes a message to the output writer.
	/// </summary>
    public static void Write(string message) => CliRunner.OutputWriter.Write(message);

	/// <summary>
	/// Writes a line to the output writer and returns the specified code.
	/// </summary>
	/// <param name="message">The message to write.</param>
	/// <param name="code">The code to return.</param>
	/// <param name="appendCode">Whether to append the code to the message.</param>
	/// <returns>A <see cref="ValueTask{TResult}"/> containing the specified code.</returns>
	/// <remarks>Using <paramref name="appendCode"/> will append [Code: <paramref name="code"/>] to <paramref name="message"/></remarks>
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