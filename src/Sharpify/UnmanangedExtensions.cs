using System.Runtime.CompilerServices;

namespace Sharpify;

public static partial class Extensions {
    /// <summary>
    /// Tries to parse an enum result from a string
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseAsEnum<TEnum>(
        this string value,
        out TEnum result) where TEnum : struct, Enum {
        return Enum.TryParse(value, out result) && Enum.IsDefined(result);
    }
}