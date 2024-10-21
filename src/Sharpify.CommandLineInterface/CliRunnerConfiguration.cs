namespace Sharpify.CommandLineInterface;

/// <summary>
/// Represents the internal configuration of a CLI runner.
/// </summary>
internal sealed class CliRunnerConfiguration {
	/// <summary>
	/// The commands that the CLI runner can execute.
	/// </summary>
	public List<Command> Commands { get; set; } = [];

	/// <summary>
	/// The metadata of the CLI runner.
	/// </summary>
	public CliMetadata MetaData { get; set; } = CliMetadata.Default;

	/// <summary>
	/// The header to use in the help text.
	/// </summary>
	public string CustomHeader { get; set; } = "";

	/// <summary>
	/// The source of the help text.
	/// </summary>
	public HelpTextSource HelpTextSource { get; set; } = HelpTextSource.Metadata;

	/// <summary>
	/// Whether to sort commands alphabetically.
	/// </summary>
	/// <remarks>
	/// It is set to false by default
	/// </remarks>
	public bool SortCommandsAlphabetically { get; set; }

	/// <summary>
	/// Whether to show error codes in the help text.
	/// </summary>
	/// <remarks>
	/// It is set to false by default to improve user experience
	/// </remarks>
	public bool ShowErrorCodes { get; set; }

	/// <summary>
	/// Whether to ignore the case of the command name.
	/// </summary>
	/// <remarks>
	/// It is set to true by default to improve user experience
	/// </remarks>
	public bool IgnoreParameterCase { get; set; } = true;

	/// <summary>
	/// Whether to output help text for empty input or print no input message.
	/// </summary>
	/// <remarks>
	/// It is set to true by default to improve user experience
	/// </remarks>
	public bool OutputHelpTextForEmptyInput { get; set; } = true;
}