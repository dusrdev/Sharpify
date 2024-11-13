namespace Sharpify.CommandLineInterface;

internal static class Helper {
	/// <summary>
	/// Tries to retrieve the value of the first specified key that exists in the dictionary.
	/// </summary>
	/// <param name="dict"></param>
	/// <param name="keys"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	internal static bool TryGetValue(this Dictionary<string, string> dict, ReadOnlySpan<string> keys, out string value) {
		foreach (var key in keys) {
			if (dict.TryGetValue(key, out var res)) {
				value = res!;
				return true;
			}
		}
		value = "";
		return false;
	}

	/// <summary>
	/// Checks if the first argument is the specified value or if it is a flag.
	/// </summary>
	/// <param name="args"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	internal static bool IsFirstOrFlag(this Arguments args, string value) {
		if (args.TryGetValue(0, out string? first) && first == value) {
			return true;
		}
		return args.Count is 1 && args.HasFlag(value);
	}

	internal static StringComparer GetComparer(this CliRunnerConfiguration config)
		=> config.ArgumentCaseHandling switch {
			ArgumentCaseHandling.IgnoreCase => StringComparer.OrdinalIgnoreCase,
			ArgumentCaseHandling.CaseSensitive => StringComparer.Ordinal,
			_ => throw new ArgumentOutOfRangeException(nameof(config.ArgumentCaseHandling), config.ArgumentCaseHandling, null)
		};
}