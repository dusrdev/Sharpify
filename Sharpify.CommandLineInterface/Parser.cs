using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sharpify.CommandLineInterface;

/// <summary>
/// Command line argument parser
/// </summary>
public static class Parser {
    /// <summary>
    /// Very efficiently splits an input into a List of strings, respects quotes
    /// </summary>
    /// <param name="str"></param>
    /// <param name="buffer">The buffer is used to reduce allocation, to overestimate, the length of str should suffice</param>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static ArraySegment<string> Split(ReadOnlySpan<char> str, string[] buffer) {
        if (str.Length is 0) {
            return ArraySegment<string>.Empty;
        }
        int pos = 0;
        int i = 0;
        while (i < str.Length) {
            char c = str[i];
            if (char.IsWhiteSpace(c)) {
                i++;
                continue;
            }
            if (c is '"') { // everything without a quote block is a single item, regardless of spaces
                str = str[(i + 1)..];
                int nextQuote = str.IndexOf('"');
                if (nextQuote is -1) {
                    break;
                }
                buffer[pos++] = new string(str[..nextQuote]);
                i = nextQuote + 1;
                continue;
            }
            // next is a word
            str = str[i..];
            int nextSpace = str.IndexOf(' ');
            if (nextSpace <= 0) { // the last word, no spaces after
                buffer[pos++] = new string(str);
                i = str.Length;
                continue;
            }
            buffer[pos++] = new string(str[..nextSpace]);
            i = nextSpace + 1;
        }
        return new ArraySegment<string>(buffer, 0, pos);
    }

    /// <summary>
    /// Splits a <see cref="ReadOnlySpan{T}"/> of characters into a list of strings.
    /// </summary>
    /// <param name="str">The input <see cref="ReadOnlySpan{T}"/> of characters to split.</param>
    /// <returns>A <see cref="List{T}"/> of strings containing the split parts.</returns>
    public static List<string> Split(ReadOnlySpan<char> str) {
        var buffer = ArrayPool<string>.Shared.Rent(str.Length);
        try {
            var argList = Split(str, buffer);
            var list = new List<string>();
            CollectionsMarshal.SetCount(list, argList.Count);
            ReadOnlySpan<string> argsSpan = argList;
            argsSpan.CopyTo(CollectionsMarshal.AsSpan(list));
            return list;
        } finally {
            buffer.ReturnBufferToSharedArrayPool();
        }
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
        var buffer = ArrayPool<string>.Shared.Rent(str.Length);
        try {
            var argList = Split(str, buffer);
            if (argList.Count is 0) {
                return null;
            }
            var args = ParseArguments(argList, comparer);
            return args;
        } finally {
            buffer.ReturnBufferToSharedArrayPool();
        }
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

        var argsCopy = GC.AllocateUninitializedArray<string>(args.Length);
        args.CopyTo(argsCopy.AsSpan());

        var results = MapArguments(argsCopy, comparer);

        return results.Count is 0 ? null : new Arguments(argsCopy, results);
    }

    // Maps a List of strings into a dictionary of arguments
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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
            var name = current[ii..]; // Parameter name without dashes

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
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static bool IsParameterName(ReadOnlySpan<char> str) {
        return str.StartsWith("-");
    }
}