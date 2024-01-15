namespace Sharpify.CommandLineInterface;

public record CliMetadata(
	string Name,
	string Description,
	string Usage,
	string Version,
	string Author,
	string License) {
	public static readonly CliMetadata Default = new(
		"Interface",
		"Default description.",
		"Interface <command> [options]",
		"1.0.0",
		"John Doe",
		"MIT");
}