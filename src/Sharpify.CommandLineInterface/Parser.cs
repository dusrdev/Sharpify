using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Sharpify.CommandLineInterface;

/// <summary>
/// Command line argument parser
/// </summary>
public static class Parser {
    /// <summary>
    /// The default starting capacity of argument buffers
    /// </summary>
    private const int DefaultBufferCapacity = 8;

    /// <summary>
    /// Very efficiently splits an input into a List of strings, respects quotes
    /// </summary>
    /// <param name="str"></param>
    public static List<string> Split(ReadOnlySpan<char> str) {
        List<string> args = new(0); // Force usage of empty array
        if (str.Length is 0) {
            return args;
        }
        args.EnsureCapacity(DefaultBufferCapacity);
        int i = 0;
        while ((uint)i < (uint)str.Length) {
            char c = str[i];
            if (char.IsWhiteSpace(c)) {
                i++;
                continue;
            }
            if (c is '"') { // everything without a quote block is a single item, regardless of spaces
                str = str.Slice(i + 1);
                int nextQuote = str.IndexOf('"');
                if (nextQuote is -1) {
                    break;
                }
                args.Add(new string(str.Slice(0, nextQuote)));
                i = nextQuote + 1;
                continue;
            }
            // next is a word
            str = str.Slice(i);
            int nextSpace = str.IndexOf(' ');
            if (nextSpace <= 0) { // the last word, no spaces after
                args.Add(new string(str));
                i = str.Length;
                continue;
            }
            args.Add(new string(str.Slice(0, nextSpace)));
            i = nextSpace + 1;
        }
        return args;
    }

    /// <summary>
    /// Parses a string into an <see cref="Arguments"/> object
    /// </summary>
    /// <param name="str"></param>
    /// <remarks>
    /// This overload uses <see cref="StringComparer.OrdinalIgnoreCase"/>
    /// </remarks>
    public static Arguments ParseArguments(ReadOnlySpan<char> str) => ParseArguments(str, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Parses a string into an <see cref="Arguments"/> object
    /// </summary>
    /// <param name="str"></param>
    /// <param name="comparer"></param>
    public static Arguments ParseArguments(ReadOnlySpan<char> str, StringComparer comparer) {
        var args = Split(str);
        return ParseArguments(args, comparer);
    }

    /// <summary>
    /// Parses a collection of strings into an <see cref="Arguments"/> object
    /// </summary>
    /// <param name="args"></param>
    /// <remarks>
    /// This overload uses <see cref="StringComparer.OrdinalIgnoreCase"/>
    /// </remarks>
    public static Arguments ParseArguments<TList>(TList args) where TList : IList<string>
        => ParseArguments(args, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Parses a collections of strings into arguments.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="comparer"></param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Arguments ParseArguments<TList>(TList args, StringComparer comparer) where TList : IList<string> {
        if (args.Count is 0) {
            return Arguments.Empty;
        }
        var roc = new ReadOnlyCollection<string>(args);
        var results = MapArguments(roc, comparer);
        return results.Count is 0 ? Arguments.Empty : new Arguments(roc, results);
    }

    // Maps a ReadOnlyCollection of strings into a dictionary of arguments
    internal static Dictionary<string, string> MapArguments(ReadOnlyCollection<string> args, StringComparer comparer) {
        var length = args.Count;
        var results = new Dictionary<string, string>(length, comparer);
        Span<bool> mapped = stackalloc bool[length];
        int i = 0;

        // Named arguments
        while (i < length) {
            var current = args[i];
            // This is positional argument, processed in the next loop
            // values of named params are processed in the single iteration of the named parameter
            if (!IsParameterName(current)) {
                i++;
                continue;
            }
            // This is parameter name (starts with either - or --)
            int ii = 0;
            while (current[ii] is '-') { // Skip the dashes
                ii++;
            }
            var name = current.Substring(ii); // Parameter name without dashes

            // i + 1 == args.Length => checks if the next argument is available
            // if not, then this is a switch (i.e. a named boolean toggle)
            // IsParameterName(args[i + 1]) => checks if the next argument is a parameter
            // if it is, then again, this is a switch
            if (i + 1 == length || IsParameterName(args[i + 1])) {
                results[name] = string.Empty;
                mapped[i] = true;
                i++;
                continue;
            }
            // If the previous condition didn't take
            // then this is the value of the named parameter
            var value = args[i + 1];
            results[name] = value;
            mapped[i] = mapped[i + 1] = true;
            i += 2;
        }

        int position = 0;

        // Positional arguments (mapped as {pos: value})
        // The positional arguments are mapped in the order they appear
        // And the number of the positional argument
        // A positional argument may have the key 0, even if it is the last enter argument (assuming other arguments are named or switches)
        for (i = 0; i < length; i++) {
            if (mapped[i]) {
                continue;
            }
            results[position.ToString()] = args[i];
            position++;
            mapped[i] = true;
        }

        return results;
    }

    // Checks whether a string starts with "-"
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool IsParameterName(ReadOnlySpan<char> str) {
        // check length
        if (str.Length is 0) {
            return false;
        }
        // check numeric + negative numeric (negative numeric could look like parameter name because of the dash)
        if (char.IsDigit(str[0]) || char.IsDigit(str[str.LastIndexOf('-') + 1])) {
            return false;
        }
        // not dash - not parameter
        if (!str.StartsWith("-")) {
            return false;
        }
        return true;
    }
}