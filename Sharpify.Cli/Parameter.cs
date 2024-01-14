using System.Globalization;

namespace Sharpify.Cli;

public readonly record struct Parameter(string Name, string Description, bool IsRequired = true, string DefaultValue = "") {
	public bool TryParse<T>(out T? val) where T : IParsable<T> {
		return T.TryParse(DefaultValue, CultureInfo.InvariantCulture, out val);
	}
}