using System.Collections.ObjectModel;

namespace Sharpify.CommandLineInterface;

/// <summary>
/// A wrapper class over a dictionary of string : string with additional features
/// </summary>
public sealed partial class Arguments {
    private readonly string[] _args;
    private readonly Dictionary<string, string> _arguments;

    /// <summary>
    /// Internal constructor for the <see cref="Arguments"/> class
    /// </summary>
    /// <param name="args">Copy or reference of the arguments before processing</param>
    /// <param name="arguments">Ensure not null or empty</param>
    internal Arguments(string[] args, Dictionary<string, string> arguments) {
        _args = args;
        _arguments = arguments;
    }

    /// <summary>
    /// Gets the number of arguments.
    /// </summary>
    public int Count => _arguments.Count;

    /// <summary>
    /// Returns a <see cref="ReadOnlyMemory{String}"/> of the arguments as they were before processing, but after splitting (if it was required)
    /// </summary>
    /// <remarks>
    /// <para>
    /// If you passed a collection of strings to be used for <see cref="Arguments"/> it will contain a copy of that array, if a <see cref="string"/> was passed, it will contain a copy of the result of <see cref="Parser.ParseArguments(ReadOnlySpan{char})"/>
    /// </para>
    /// <para>
    /// In normal use case you shouldn't need this, but in case you want to manufacture some sort of a nested command structure, you can use this to filter once more for <see cref="Arguments"/> after selectively parsing some of the arguments, in which case it is very powerful.
    /// </para>
    /// </remarks>
    public ReadOnlyMemory<string> ArgsAsMemory() => _args;

    /// <summary>
    /// Returns a <see cref="ReadOnlySpan{String}"/> of the arguments as they were before processing, but after splitting (if it was required)
    /// </summary>
    /// <remarks>
    /// <para>
    /// If you passed a collection of strings to be used for <see cref="Arguments"/> it will contain a copy of that array, if a <see cref="string"/> was passed, it will contain a copy of the result of <see cref="Parser.ParseArguments(ReadOnlySpan{char})"/>
    /// </para>
    /// <para>
    /// In normal use case you shouldn't need this, but in case you want to manufacture some sort of a nested command structure, you can use this to filter once more for <see cref="Arguments"/> after selectively parsing some of the arguments, in which case it is very powerful.
    /// </para>
    /// </remarks>
    public ReadOnlySpan<string> ArgsAsSpan() => _args;

    /// <summary>
    /// Returns new Arguments with positional arguments forwarded by 1, so that argument that was 1 is now 0, 2 is now 1 and so on. This is non-destructive, the original arguments are not modified.
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
            return new(_args, _arguments);
        }
        var dict = new Dictionary<string, string>(_arguments.Comparer);

        foreach (var (prevK, prevV) in _arguments) {
            // Handle non numeric
            if (!int.TryParse(prevK.AsSpan(), out int numericIndex)) {
                dict.Add(prevK, prevV);
            }
            // Handle numeric
            if (numericIndex is 0) { // forwarding means the previous 0 is lost
                continue;
            }
            dict.Add((numericIndex - 1).ToString(), prevV); // Add with the index reduced by 1.
        }

        // Because this is a new dictionary, if pos 1, isn't found, 0 still won't be present
        // So essentially 0 was forwarded to no longer exist
        return new(_args, dict);
    }

    /// <summary>
    /// Returns the underlying dictionary
    /// </summary>
    public ReadOnlyDictionary<string, string> GetInnerDictionary() => _arguments.AsReadOnly();
}
