using System.Collections.ObjectModel;

namespace Sharpify.CommandLineInterface;

/// <summary>
/// A wrapper class over a dictionary of string : string with additional features
/// </summary>
/// <remarks>
/// Arguments instances are created via <see cref="Parser"/>
/// </remarks>
public sealed partial class Arguments {
    /// <summary>
    /// Source is the list of separated arguments on top of which this instance of <see cref="Arguments"/> was built.
    /// </summary>
    public readonly ReadOnlyCollection<string> Source;
    private readonly Dictionary<string, string> _arguments;

    /// <summary>
    /// Internal constructor for the <see cref="Arguments"/> class
    /// </summary>
    /// <param name="args">Copy or reference of the arguments before processing</param>
    /// <param name="arguments">Ensure not null or empty</param>
    internal Arguments(ReadOnlyCollection<string> args, Dictionary<string, string> arguments) {
        Source = args;
        _arguments = arguments;
    }

    /// <summary>
    /// Gets the number of arguments.
    /// </summary>
    public int Count => _arguments.Count;

    /// <summary>
    /// Checks if the arguments are empty.
    /// </summary>
    public bool AreEmpty => _arguments.Count is 0;

    /// <summary>
    /// Returns an empty arguments object.
    /// </summary>
    public static readonly Arguments Empty = new(Array.Empty<string>().AsReadOnly(), []);

    /// <summary>
    /// Returns an array copy of <see cref="Source"/>
    /// </summary>
    public string[] SourceCopy => Source.ToArray();

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
            return this;
        }
        var dict = new Dictionary<string, string>(_arguments.Comparer);

        foreach ((string prevKey, string prevValue) in _arguments) {
            // Handle non numeric
            if (!int.TryParse(prevKey.AsSpan(), out int numericIndex)) {
                dict.Add(prevKey, prevValue);
            }
            // Handle numeric
            if (numericIndex is 0) { // forwarding means the previous 0 is lost
                continue;
            }
            dict.Add((numericIndex - 1).ToString(), prevValue); // Add with the index reduced by 1.
        }

        // Because this is a new dictionary, if pos 1, isn't found, 0 still won't be present
        // So essentially 0 was forwarded to no longer exist
        return new Arguments(Source, dict);
    }

    /// <summary>
    /// Returns the underlying dictionary
    /// </summary>
    public ReadOnlyDictionary<string, string> GetInnerDictionary() => _arguments.AsReadOnly();
}
