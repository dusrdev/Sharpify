namespace Sharpify.CommandLineInterface.Tests;

public static class Helper {
	public static Dictionary<string, string> GetMapped(params (string, string)[] parameters) {
		var dict = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
		foreach (var (key, value) in parameters) {
			dict[key] = value;
		}
		return dict;
	}
}