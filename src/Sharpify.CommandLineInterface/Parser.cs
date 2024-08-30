using System.Runtime.CompilerServices;

using Sharpify.Collections;

namespace Sharpify.CommandLineInterface;

/// <summary>
/// Command line argument parser
/// </summary>
public static class Parser {
    /// <summary>
    /// Very efficiently splits an input into a List of strings, respects quotes
    /// </summary>
    /// <param name="str"></param>
    public static RentedBufferWriter<string> Split(ReadOnlySpan<char> str) {
        if (str.Length is 0) {
            return new RentedBufferWriter<string>(0);
        }
        var buffer = new RentedBufferWriter<string>(str.Length);
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
                buffer.WriteAndAdvance(new string(str.Slice(0, nextQuote)));
                i = nextQuote + 1;
                continue;
            }
            // next is a word
            str = str.Slice(i);
            int nextSpace = str.IndexOf(' ');
            if (nextSpace <= 0) { // the last word, no spaces after
                buffer.WriteAndAdvance(new string(str));
                i = str.Length;
                continue;
            }
            buffer.WriteAndAdvance(new string(str.Slice(0, nextSpace)));
            i = nextSpace + 1;
        }
        return buffer;
    }

    /// <summary>
    /// Splits a <see cref="ReadOnlySpan{T}"/> of characters into a list of strings.
    /// </summary>
    /// <param name="str">The input <see cref="ReadOnlySpan{T}"/> of characters to split.</param>
    /// <returns>A <see cref="List{T}"/> of strings containing the split parts.</returns>
    public static List<string> SplitToList(ReadOnlySpan<char> str) {
        using var splitBuffer = Split(str);
        var span = splitBuffer.WrittenSpan;
        var list = new List<string>(span.Length);
        list.AddRange(span);
        return list;
    }

    /// <summary>
    /// Parses a string into an <see cref="Arguments"/> object
    /// </summary>
    /// <param name="str"></param>
    public static Arguments? ParseArguments(ReadOnlySpan<char> str) => ParseArguments(str, StringComparer.CurrentCultureIgnoreCase);

    /// <summary>
    /// Parses a string into an <see cref="Arguments"/> object
    /// </summary>
    /// <param name="str"></param>
    /// <param name="comparer"></param>
    public static Arguments? ParseArguments(ReadOnlySpan<char> str, StringComparer comparer) {
        using var splitBuffer = Split(str);
        return ParseArguments(splitBuffer.WrittenSpan, comparer);
    }

    /// <summary>
    /// Parses an List of strings into an <see cref="Arguments"/> object
    /// </summary>
    /// <param name="args"></param>
    /// <param name="comparer"></param>
    public static Arguments? ParseArguments(List<string> args, StringComparer comparer) => ParseArgumentsInternal(args.AsSpan(), comparer);

    /// <summary>
    /// Parses a ReadOnlySpan of strings into arguments.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="comparer"></param>
    public static Arguments? ParseArguments(ReadOnlySpan<string> args, StringComparer comparer) => ParseArgumentsInternal(args, comparer);

    // Parses a List<string> into a dictionary of arguments
    internal static Arguments? ParseArgumentsInternal(ReadOnlySpan<string> args, StringComparer comparer) {
        if (args.Length is 0) {
            return null;
        }

        var argsCopy = args.ToArray();
        var results = MapArguments(argsCopy, comparer);
        return results.Count is 0 ? null : new Arguments(argsCopy, results);
    }

    // Maps a List of strings into a dictionary of arguments
    internal static Dictionary<string, string> MapArguments(ReadOnlySpan<string> args, StringComparer comparer) {
        var results = new Dictionary<string, string>(args.Length, comparer);
        Span<bool> mapped = stackalloc bool[args.Length];
        int i = 0;

        // Named arguments
        while (i < args.Length) {
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
            if (i + 1 == args.Length || IsParameterName(args[i + 1])) {
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
        for (i = 0; i < args.Length; i++) {
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsParameterName(ReadOnlySpan<char> str) => str.StartsWith("-");
}