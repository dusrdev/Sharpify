namespace Sharpify.CommandLineInterface;

/// <summary>
/// Contains metadata for a CLI application.
/// </summary>
public record CliMetadata {
	/// <summary>
	/// The name of the CLI application.
	/// </summary>
	public string Name { get; set; } = "";

	/// <summary>
	/// The description of the CLI application.
	/// </summary>
	public string Description { get; set; } = "";

	/// <summary>
	/// The version of the CLI application.
	/// </summary>
	public string Version { get; set; } = "";

	/// <summary>
	/// The author of the CLI application.
	/// </summary>
	public string Author { get; set; } = "";

	/// <summary>
	/// The license of the CLI application.
	/// </summary>
 	public string License { get; set; } = "";

	/// <summary>
	/// Whether or not to include the metadata in the help text.
	/// </summary>
	public bool IncludeInHelpText { get; set; } = true;

	/// <summary>
	/// The default metadata for a CLI application.
	/// </summary>
	public static readonly CliMetadata Default = new() {
		Name = "Interface",
		Description = "Default description.",
		Version = "1.0.0",
		Author = "John Doe",
		License = "MIT",
		IncludeInHelpText = true
	};

	/// <summary>
	/// Returns the total length of the metadata.
	/// </summary>
	public int TotalLength => Name.Length + Description.Length + Version.Length + Author.Length + License.Length;
}