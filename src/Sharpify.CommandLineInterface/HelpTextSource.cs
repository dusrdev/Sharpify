namespace Sharpify.CommandLineInterface;

/// <summary>
/// Dictates the source of the general help text
/// </summary>
public enum HelpTextSource {
	/// <summary>
	/// Use the metadata to generate HelpText
	/// </summary>
	Metadata,
	/// <summary>
	/// Use the custom header to generate HelpText
	/// </summary>
	CustomHeader
}