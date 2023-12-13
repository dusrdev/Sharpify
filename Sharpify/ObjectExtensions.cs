using System.Runtime.CompilerServices;

namespace Sharpify;

public static partial class Extensions {
    /// <summary>
    /// Attempts to unbox an object to a specified value type.
    /// </summary>
    /// <typeparam name="T">The value type to unbox to.</typeparam>
    /// <param name="obj">The object to unbox.</param>
    /// <param name="value">When this method returns, contains the unboxed value if the unboxing is successful; otherwise, the default value of <typeparamref name="T"/>.</param>
    /// <returns><c>true</c> if the unboxing is successful; otherwise, <c>false</c>.</returns>
    /// <remarks>Copied from CommunityToolkit.HighPerformance</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryUnbox<T>(this object obj, out T value)
        where T : struct
    {
        if (obj.GetType() == typeof(T))
        {
            value = Unsafe.Unbox<T>(obj);
            return true;
        }

        value = default;
        return false;
    }
}