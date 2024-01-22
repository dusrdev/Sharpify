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
            if (c is '"') {
                str = str[(i + 1)..];
                int nextQuote = str.IndexOf('"');
                if (nextQuote is -1) {
                    break;
                }
                buffer[pos++] = new string(str[..nextQuote]);
                i = nextQuote + 1;
                continue;
            }
            str = str[i..];
            int nextSpace = str.IndexOf(' ');
            if (nextSpace <= 0) {
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
    private static Arguments? ParseArgumentsInternal(ReadOnlySpan<string> args, StringComparer comparer) {
        if (args.Length is 0) {
            return null;
        }

        Memory<string> argsCopy = new string[args.Length];
        args.CopyTo(argsCopy.Span);

        var results = new Dictionary<string, string>(args.Length, comparer);
        int i = 0;

        while (i < args.Length && !IsParameterName(args[i])) {
            results[i.ToString()] = args[i];
            i++;
        }

        while (i < args.Length) {
            var current = args[i];
            // Ignore string as it is invalid parameter name
            if (!IsParameterName(current)) {
                i++;
                continue;
            }
            // Parameter name
            int ii = 0;
            while (current[ii] is '-') {
                ii++;
            }
            // Next is unavailable or another parameter
            if (i + 1 == args.Length || IsParameterName(args[i + 1])) {
                results[current[ii..]] = string.Empty;
                i++;
                continue;
            }
            // Next is available and not a parameter but rather a value
            results[current[ii..]] = args[i + 1];
            i += 2;
        }

        return results.Count is 0 ? null : new Arguments(argsCopy, results);
    }

    // Checks whether a string starts with "-"
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsParameterName(ReadOnlySpan<char> str) {
        return str.StartsWith("-");
    }
}