namespace Sharpify.CommandLineInterface;

/// <summary>
/// Controls how the CLI runner handles empty input.
/// </summary>
public enum EmptyInputBehavior {
	/// <summary>
	/// Displays the help text and exits.
	/// </summary>
	DisplayHelpText,
	/// <summary>
	/// Attempts to proceed with handling the commands.
	/// </summary>
	/// <remarks>
	/// If a single command is used and command name is set to not required, this will execute the command with empty args,
	/// otherwise it will display the appropriate error message.
	/// </remarks>
	AttemptToProceed,
}

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

/// <summary>
/// Configures how to handle argument casing
/// </summary>
public enum ArgumentCaseHandling {
	/// <summary>
	/// Ignore argument case
	/// </summary>
	IgnoreCase,
	/// <summary>
	/// Sets the arguments parser to be case sensitive
	/// </summary>
	CaseSensitive
}