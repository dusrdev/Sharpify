namespace Sharpify.CommandLineInterface;

/// <summary>
/// Contains metadata for a CLI application.
/// </summary>
public record CliMetadata {
	/// <summary>
	/// The name of the CLI application.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// The description of the CLI application.
	/// </summary>
	public string Description { get; set; } = string.Empty;

	/// <summary>
	/// The version of the CLI application.
	/// </summary>
	public string Version { get; set; } = string.Empty;

	/// <summary>
	/// The author of the CLI application.
	/// </summary>
	public string Author { get; set; } = string.Empty;

	/// <summary>
	/// The license of the CLI application.
	/// </summary>
 	public string License { get; set; } = string.Empty;

	/// <summary>
	/// The default metadata for a CLI application.
	/// </summary>
	public static readonly CliMetadata Default = new() {
		Name = "Interface",
		Description = "Default description.",
		Version = "1.0.0",
		Author = "John Doe",
		License = "MIT",
	};

	/// <summary>
	/// Returns the total length of the metadata.
	/// </summary>
	public int TotalLength => Name.Length + Description.Length + Version.Length + Author.Length + License.Length;
}