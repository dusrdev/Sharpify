namespace Sharpify.CommandLineInterface;

internal static class Extensions {
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

	internal static StringComparer GetComparer(this CliRunnerConfiguration config)
		=> config.IgnoreParameterCase
		? StringComparer.OrdinalIgnoreCase
		: StringComparer.Ordinal;
}