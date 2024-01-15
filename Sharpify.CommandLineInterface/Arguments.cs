using System.Collections.ObjectModel;
using System.Globalization;

namespace Sharpify.CommandLineInterface;

/// <summary>
/// A wrapper class over a dictionary of string : string with additional features
/// </summary>
public sealed class Arguments {
    private readonly Dictionary<string, string> _arguments;

    /// <summary>
    /// Internal constructor for the <see cref="Arguments"/> class
    /// </summary>
    /// <param name="arguments">Ensure not null or empty</param>
    internal Arguments(Dictionary<string, string> arguments) {
        _arguments = arguments;
    }

    /// <summary>
    /// Gets the number of arguments.
    /// </summary>
    public int Count => _arguments.Count;

    /// <summary>
    /// Checks if the specified key exists in the arguments.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key exists, false otherwise.</returns>
    public bool Contains(string key) => _arguments.ContainsKey(key);

	/// <summary>
	/// Tries to retrieve the value of a positional argument.
	/// </summary>
	/// <param name="key">The key to check.</param>
	/// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
	/// <returns>true if the key exists, false otherwise.</returns>
	public bool TryGetValue(int key, out string value) => TryGetValue(key.ToString(), out value);

    /// <summary>
    /// Tries to retrieve the value of a specified key in the arguments.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
	/// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetValue(string key, out string value) {
        if (!_arguments.TryGetValue(key, out var res)) {
			value = "";
			return false;
		}
		value = res!;
		return true;
    }

    /// <summary>
    /// Tries to retrieve the value of a specified key in the arguments.
    /// </summary>
	/// <param name="key">The positional argument to check.</param>
	/// <param name="defaultValue">The default value to return if the key doesn't exist.</param>
	/// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
	/// <remarks>
	/// If the key doesn't exist or can't be parsed, the default value will be used in the out parameter.
	/// </remarks>
	/// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetValue<T>(int key, T defaultValue, out T value) where T : IParsable<T> => TryGetValue(key.ToString(), defaultValue, out value);

    /// <summary>
    /// Tries to retrieve the value of a specified key in the arguments.
    /// </summary>
	/// <param name="key">The key to check.</param>
	/// <param name="defaultValue">The default value to return if the key doesn't exist.</param>
	/// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
	/// <remarks>
	/// If the key doesn't exist or can't be parsed, the default value will be used in the out parameter.
	/// </remarks>
	/// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetValue<T>(string key, T defaultValue, out T value) where T : IParsable<T> {
        if (!TryGetValue(key, out string val)) {
			value = defaultValue;
			return false;
		}
        var wasParsed = T.TryParse(val, CultureInfo.CurrentCulture, out T? result);
        if (!wasParsed) {
            value = defaultValue;
			return false;
        }
		value = result!;
        return true;
    }

    /// <summary>
	/// Tries to retrieve the enum value of a specified key in the arguments.
	/// </summary>
	/// <param name="key">The positional argument to check.</param>
	/// <param name="defaultValue">The default value to return if the key doesn't exist.</param>
	/// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
	/// <remarks>
	/// If the key doesn't exist or can't be parsed, the default value will be used in the out parameter.
	/// </remarks>
	/// <returns>true if the key exists, false otherwise.</returns>
	public bool TryGetEnum<TEnum>(int key, TEnum defaultValue, out TEnum value) where TEnum : struct, Enum => TryGetEnum(key.ToString(), defaultValue, out value);

	/// <summary>
	/// Tries to retrieve the enum value of a specified key in the arguments.
	/// </summary>
	/// <param name="key">The key to check.</param>
	/// <param name="defaultValue">The default value to return if the key doesn't exist.</param>
	/// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
	/// <remarks>
	/// If the key doesn't exist or can't be parsed, the default value will be used in the out parameter.
	/// </remarks>
	/// <returns>true if the key exists, false otherwise.</returns>
	public bool TryGetEnum<TEnum>(string key, TEnum defaultValue, out TEnum value) where TEnum : struct, Enum {
		if (!TryGetValue(key, out string val)) {
			value = defaultValue;
			return false;
		}
		var wasParsed = val!.TryParseAsEnum(out TEnum result);
		if (!wasParsed) {
			value = defaultValue;
			return false;
		}
		value = result;
		return true;
	}

    /// <summary>
    /// Returns Arguments with positional arguments forwarded by 1, so that argument that was 1 is now 0, 2 is now 1 and so on
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is useful if you have a command that has a sub-command and you want to pass the arguments to the sub-command
    /// </para>
    /// <para>
    /// The first positional argument (0) will be skipped to actually forward
    /// </para>
    /// </remarks>
    public Arguments ForwardPositionalArguments() {
        if (!Contains("0")) {
            return new(_arguments);
        }
        var dict = new Dictionary<string, string>(_arguments.Comparer);
        // We start with 1 to delete the first argument
        for (int i = 1; _arguments.TryGetValue(i.ToString(), out string? value); i++) {
            dict[(i - 1).ToString()] = value;
        }
        return new(dict);
    }

    /// <summary>
    /// Returns the underlying dictionary
    /// </summary>
    public ReadOnlyDictionary<string, string> GetInnerDictionary() => _arguments.AsReadOnly();
}
