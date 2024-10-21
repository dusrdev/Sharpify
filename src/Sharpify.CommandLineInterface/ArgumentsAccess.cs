using System.Globalization;

namespace Sharpify.CommandLineInterface;

public sealed partial class Arguments {
    /// <summary>
    /// Checks if the specified key exists in the arguments.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key exists, false otherwise.</returns>
    public bool Contains(string key) => _arguments.ContainsKey(key);

    /// <summary>
    /// Checks if the specified positional argument exists in the arguments.
    /// </summary>
    /// <param name="position">The positional argument to check.</param>
    /// <returns>True if the key exists, false otherwise.</returns>
    public bool Contains(int position) => Contains(position.ToString());

    /// <summary>
    /// Checks if the specified flag is present in the arguments.
    /// </summary>
    /// <param name="flag">The flag to check.</param>
    /// <returns>True if the flag is present and has no value; otherwise, false.</returns>
    /// <remarks>
    /// This is not the same as <see cref="Contains(string)"/> as this also checks that the value is empty, which is not the case for named arguments that can also be detected by <see cref="Contains(string)"/>
    /// </remarks>
    public bool HasFlag(string flag) => _arguments.TryGetValue(flag, out string? val) && val.Length is 0;

    /// <summary>
    /// Tries to retrieve the value of a positional argument.
    /// </summary>
    /// <param name="position">The key to check.</param>
    /// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
    /// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetValue(int position, out string value) => TryGetValue(position.ToString(), out value);

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
    /// Tries to retrieve a value of one of a specified key's aliases in the arguments.
    /// </summary>
    /// <param name="keys">A collection of aliases for a parameter name</param>
    /// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
    /// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetValue(ReadOnlySpan<string> keys, out string value)
        => _arguments.TryGetValue(keys, out value);

    /// <summary>
    /// Tries to retrieve the value of the positional argument in the arguments.
    /// </summary>
	/// <param name="position">The positional argument to check.</param>
	/// <param name="defaultValue">The default value to return if the key doesn't exist.</param>
	/// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
	/// <remarks>
    /// <para>
	/// If the key doesn't exist or can't be parsed, the default value will be used in the out parameter.
    /// </para>
    /// <para>
    /// The default value makes it very easy to default to some value that is used later even if not provided, for example: a downloader may accept the number of parallel connections as a parameter, but it should always default to some number, so you could put it here. Saving a few lines of code for checking and reverting to default yourself.
    /// </para>
	/// </remarks>
	/// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetValue<T>(int position, T defaultValue, out T value) where T : IParsable<T> => TryGetValue(position.ToString(), defaultValue, out value);

    /// <summary>
    /// Tries to retrieve the value of a specified key in the arguments.
    /// </summary>
	/// <param name="key">The key to check.</param>
	/// <param name="defaultValue">The default value to return if the key doesn't exist.</param>
	/// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
	/// <remarks>
	/// <para>
	/// If the key doesn't exist or can't be parsed, the default value will be used in the out parameter.
    /// </para>
    /// <para>
    /// The default value makes it very easy to default to some value that is used later even if not provided, for example: a downloader may accept the number of parallel connections as a parameter, but it should always default to some number, so you could put it here. Saving a few lines of code for checking and reverting to default yourself.
    /// </para>
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
    /// Returns the value of the argument, or default if it fails to parse or key didn't exist.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">The key of the argument to check</param>
    /// <param name="defaultValue">The default value to return if the argument doesn't exist or can't be parsed</param>
    /// <returns>The value of the argument</returns>
    public T GetValue<T>(string key, T defaultValue) where T : IParsable<T> {
        _ = TryGetValue(key, defaultValue, out var value);
        return value;
    }

    /// <summary>
    /// Returns the value of the positional argument, or default if it fails to parse or key didn't exist.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="position">The position of the argument to check</param>
    /// <param name="defaultValue">The default value to return if the argument doesn't exist or can't be parsed</param>
    /// <returns>Value of the argument</returns>
    public T GetValue<T>(int position, T defaultValue) where T : IParsable<T> {
        _ = TryGetValue(position, defaultValue, out var value);
        return value;
    }

    /// <summary>
    /// Tries to retrieve the value of a either of specified key's aliases in the arguments.
    /// </summary>
	/// <param name="keys">A collection of aliases for a parameter name</param>
	/// <param name="defaultValue">The default value to return if the key doesn't exist.</param>
	/// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
	/// <remarks>
	/// <para>
	/// If the key doesn't exist or can't be parsed, the default value will be used in the out parameter.
    /// </para>
    /// <para>
    /// The default value makes it very easy to default to some value that is used later even if not provided, for example: a downloader may accept the number of parallel connections as a parameter, but it should always default to some number, so you could put it here. Saving a few lines of code for checking and reverting to default yourself.
    /// </para>
	/// </remarks>
	/// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetValue<T>(ReadOnlySpan<string> keys, T defaultValue, out T value) where T : IParsable<T> {
        if (!TryGetValue(keys, out string val)) {
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
    /// Returns the value of the either of the key aliases, or default if it fails to parse or key didn't exist.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="keys"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public T GetValue<T>(ReadOnlySpan<string> keys, T defaultValue) where T : IParsable<T> {
        _ = TryGetValue(keys, defaultValue, out var value);
        return value;
    }

    /// <summary>
	/// Tries to retrieve the enum value of a specified key in the arguments.
	/// </summary>
	/// <param name="position">The positional argument to check.</param>
	/// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
	/// <remarks>
	/// If the key doesn't exist or can't be parsed, the the default(TEnum) will be used in the out parameter, this overloads also implies that the enum will be parsed case-sensitive
	/// </remarks>
	/// <returns>true if the key exists, false otherwise.</returns>
	public bool TryGetEnum<TEnum>(int position, out TEnum value) where TEnum : struct, Enum => TryGetEnum(position.ToString(), default, false, out value);

    /// <summary>
	/// Tries to retrieve the enum value of a specified key in the arguments.
	/// </summary>
	/// <param name="position">The positional argument to check.</param>
    /// <param name="ignoreCase">Whether to ignore case in parsing the enum</param>
	/// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
	/// <remarks>
	/// If the key doesn't exist or can't be parsed, the default(TEnum) will be used in the out parameter.
	/// </remarks>
	/// <returns>true if the key exists, false otherwise.</returns>
	public bool TryGetEnum<TEnum>(int position, bool ignoreCase, out TEnum value) where TEnum : struct, Enum => TryGetEnum(position.ToString(), default, ignoreCase, out value);

    /// <summary>
	/// Tries to retrieve the enum value of a specified key in the arguments.
	/// </summary>
	/// <param name="position">The positional argument to check.</param>
	/// <param name="defaultValue">The default value to return if the key doesn't exist.</param>
    /// <param name="ignoreCase">Whether to ignore case in parsing the enum</param>
	/// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
	/// <remarks>
	/// If the key doesn't exist or can't be parsed, the default value will be used in the out parameter.
	/// </remarks>
	/// <returns>true if the key exists, false otherwise.</returns>
	public bool TryGetEnum<TEnum>(int position, TEnum defaultValue, bool ignoreCase, out TEnum value) where TEnum : struct, Enum => TryGetEnum(position.ToString(), defaultValue, ignoreCase, out value);

    /// <summary>
    /// Tries to retrieve the enum value of a specified key in the arguments.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
    /// <remarks>
    /// If the key doesn't exist or can't be parsed, the default(TEnum) will be used in the out parameter, this overloads also implies that the enum will be parsed case-sensitive
    /// </remarks>
    /// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetEnum<TEnum>(string key, out TEnum value) where TEnum : struct, Enum =>
        TryGetEnum(key, default, false, out value);

    /// <summary>
    /// Tries to retrieve the enum value of a specified key in the arguments.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
    /// <param name="ignoreCase">Whether to ignore case in parsing the enum</param>
    /// <remarks>
    /// If the key doesn't exist or can't be parsed, the default(TEnum) will be used in the out parameter.
    /// </remarks>
    /// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetEnum<TEnum>(string key, bool ignoreCase, out TEnum value) where TEnum : struct, Enum =>
        TryGetEnum(key, default, ignoreCase, out value);

    /// <summary>
    /// Tries to retrieve the enum value of a specified key in the arguments.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
    /// <param name="defaultValue">The default value to return if the key doesn't exist.</param>
    /// <param name="ignoreCase">Whether to ignore case in parsing the enum</param>
    /// <remarks>
    /// If the key doesn't exist or can't be parsed, the default value will be used in the out parameter.
    /// </remarks>
    /// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetEnum<TEnum>(string key, TEnum defaultValue, bool ignoreCase, out TEnum value) where TEnum : struct, Enum {
        if (!TryGetValue(key, out string val)) {
            value = defaultValue;
            return false;
        }
        var wasParsed = Enum.TryParse(val, ignoreCase, out TEnum result);
        if (!wasParsed) {
            value = defaultValue;
            return false;
        }
        value = result;
        return true;
    }

    /// <summary>
    /// Tries to retrieve the enum value of one of a specified key's aliases in the arguments.
    /// </summary>
    /// <param name="keys">A collection of aliases for a parameter name</param>
    /// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
    /// <remarks>
    /// If the key doesn't exist or can't be parsed, the default value will be used in the out parameter.
    /// </remarks>
    /// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetEnum<TEnum>(ReadOnlySpan<string> keys, out TEnum value) where TEnum : struct, Enum
        => TryGetEnum(keys, default, false, out value);

    /// <summary>
    /// Tries to retrieve the enum value of one of a specified key's aliases in the arguments.
    /// </summary>
    /// <param name="keys">A collection of aliases for a parameter name</param>
    /// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
    /// <param name="ignoreCase">Whether to ignore case in parsing the enum</param>
    /// <remarks>
    /// If the key doesn't exist or can't be parsed, the default value will be used in the out parameter.
    /// </remarks>
    /// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetEnum<TEnum>(ReadOnlySpan<string> keys, bool ignoreCase, out TEnum value) where TEnum : struct, Enum
        => TryGetEnum(keys, default, ignoreCase, out value);

    /// <summary>
    /// Tries to retrieve the enum value of one of a specified key's aliases in the arguments.
    /// </summary>
    /// <param name="keys">A collection of aliases for a parameter name</param>
    /// <param name="value">The value of the argument ("" if doesn't exist - NOT NULL).</param>
    /// <param name="defaultValue">The default value to return if the key doesn't exist.</param>
    /// <param name="ignoreCase">Whether to ignore case in parsing the enum</param>
    /// <remarks>
    /// If the key doesn't exist or can't be parsed, the default value will be used in the out parameter.
    /// </remarks>
    /// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetEnum<TEnum>(ReadOnlySpan<string> keys, TEnum defaultValue, bool ignoreCase, out TEnum value) where TEnum : struct, Enum {
        if (!TryGetValue(keys, out string val)) {
            value = defaultValue;
            return false;
        }
        var wasParsed = Enum.TryParse(val, ignoreCase, out TEnum result);
        if (!wasParsed) {
            value = defaultValue;
            return false;
        }
        value = result;
        return true;
    }

    /// <summary>
    /// Returns the enum value of a positional argument, or default if it fails to parse or didn't exist.
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="position">The position of the argument to check</param>
    /// <param name="defaultValue">The default value to return if the argument doesn't exist or can't be parsed</param>
    /// <returns>The value of the argument</returns>
    public TEnum GetEnum<TEnum>(int position, TEnum defaultValue) where TEnum : struct, Enum {
        _ = TryGetEnum(position, defaultValue, false, out var value);
        return value;
    }

    /// <summary>
    /// Returns the enum value of a positional argument, or default if it fails to parse or didn't exist.
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="position">The position of the argument to check</param>
    /// <param name="defaultValue">The default value to return if the argument doesn't exist or can't be parsed</param>
    /// <param name="ignoreCase">True to ignore case when parsing the argument</param>
    /// <returns>The value of the argument</returns>
    public TEnum GetEnum<TEnum>(int position, TEnum defaultValue, bool ignoreCase) where TEnum : struct, Enum {
        _ = TryGetEnum(position, defaultValue, ignoreCase, out var value);
        return value;
    }

    /// <summary>
    /// Returns the enum value of a specified key, or default if it fails to parse or key didn't exist.
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="key">The key of the argument to check</param>
    /// <param name="defaultValue">The default value to return if the argument doesn't exist or can't be parsed</param>
    /// <returns>The value of the argument</returns>
    public TEnum GetEnum<TEnum>(string key, TEnum defaultValue) where TEnum : struct, Enum {
        _ = TryGetEnum(key, defaultValue, false, out var value);
        return value;
    }

    /// <summary>
    /// Returns the enum value of a specified key, or default if it fails to parse or key didn't exist.
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="key">The key of the argument to check</param>
    /// <param name="defaultValue">The default value to return if the argument doesn't exist or can't be parsed</param>
    /// <param name="ignoreCase">True to ignore case when parsing the argument</param>
    /// <returns>The value of the argument</returns>
    public TEnum GetEnum<TEnum>(string key, TEnum defaultValue, bool ignoreCase) where TEnum : struct, Enum {
        _ = TryGetEnum(key, defaultValue, ignoreCase, out var value);
        return value;
    }

    /// <summary>
    /// Returns the value of the either of the key aliases, or default if it fails to parse or key didn't exist.
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="keys">The keys of the argument to check</param>
    /// <param name="defaultValue">The default value to return if the argument doesn't exist or can't be parsed</param>
    /// <returns>The value of the argument</returns>
    public TEnum GetEnum<TEnum>(ReadOnlySpan<string> keys, TEnum defaultValue) where TEnum : struct, Enum {
        _ = TryGetEnum(keys, defaultValue, false, out var value);
        return value;
    }

    /// <summary>
    /// Returns the value of the either of the key aliases, or default if it fails to parse or key didn't exist.
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="keys">The keys of the argument to check</param>
    /// <param name="defaultValue">The default value to return if the argument doesn't exist or can't be parsed</param>
    /// <param name="ignoreCase">True to ignore case when parsing the argument</param>
    /// <returns>The value of the argument</returns>
    public TEnum GetEnum<TEnum>(ReadOnlySpan<string> keys, TEnum defaultValue, bool ignoreCase) where TEnum : struct, Enum {
        _ = TryGetEnum(keys, defaultValue, ignoreCase, out var value);
        return value;
    }
}
