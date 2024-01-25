namespace Sharpify.CommandLineInterface;

[Flags]
internal enum CliRunnerOptions {
	// Whether to include Metadata in the help text
	IncludeMetadata = 1,
	// Whether to sort commands alphabetically (the difference is only in the help text)
	SortCommandsAlphabetically = 1 << 1,
	// Whether to use a custom header for the help text
	UseCustomHeader = 1 << 2,
}