namespace Sharpify.CommandLineInterface;

/// <summary>
/// Contains metadata for a CLI application.
/// </summary>
public record CliMetadata {
	/// <summary>
	/// The name of the CLI application.
	/// </summary>
	public string Name { get; init; } = "";

	/// <summary>
	/// The description of the CLI application.
	/// </summary>
	public string Description { get; init; } = "";

	/// <summary>
	/// The version of the CLI application.
	/// </summary>
	public string Version { get; init; } = "";

	/// <summary>
	/// The author of the CLI application.
	/// </summary>
	public string Author { get; init; } = "";

	/// <summary>
	/// The license of the CLI application.
	/// </summary>
 	public string License { get; init; } = "";

	/// <summary>
	/// The default metadata for a CLI application.
	/// </summary>
	public static readonly CliMetadata Default = new() {
		Name = "Interface",
		Description = "Default description.",
		Version = "1.0.0",
		Author = "John Doe",
		License = "MIT"
	};
}