using System.Runtime.InteropServices;

using U = System.Runtime.CompilerServices.Unsafe;

namespace Sharpify;

public static partial class Utils {
    /// <summary>
    /// Provides utility unsafe utility methods for utilization of other high performance apis
    /// </summary>
    public static class Unsafe {
        /// <summary>
        /// Creates an integer predicate from a given predicate function.
        /// </summary>
        /// <typeparam name="T">The type of the input parameter.</typeparam>
        /// <param name="predicate">The predicate function.</param>
        /// <returns>An integer predicate. 1 if the original predicate would've return true, otherwise 0</returns>
        /// <remarks>
        /// This allows usage of a predicate to count elements that match a given condition, using hardware intrinsics to speed up the process.
        /// The integer return value allows to use this converted function with IEnumerable{T}.Sum which is a hardware accelerated method, but the result will be identical to calling Count(predicate).
        /// </remarks>
        public static Func<T, int> CreateIntegerPredicate<T>(Func<T, bool> predicate) =>
            U.As<Func<T, bool>, Func<T, int>>(ref predicate);

        /// <summary>
        /// Converts a read-only span to a mutable span.
        /// </summary>
        /// <typeparam name="T">The type of elements in the span.</typeparam>
        /// <param name="span">The read-only span to convert.</param>
        /// <returns>A mutable span.</returns>
        public static unsafe Span<T> AsMutableSpan<T>(ReadOnlySpan<T> span) {
            ref var p = ref MemoryMarshal.GetReference(span);
            void* pointer = U.AsPointer(ref p);
            return new Span<T>(pointer, span.Length);
        }

        /// <summary>
        /// Attempts to unbox an object to a specified value type.
        /// </summary>
        /// <typeparam name="T">The value type to unbox to.</typeparam>
        /// <param name="obj">The object to unbox.</param>
        /// <param name="value">When this method returns, contains the unboxed value if the unboxing is successful; otherwise, the default value of <typeparamref name="T"/>.</param>
        /// <returns><c>true</c> if the unboxing is successful; otherwise, <c>false</c>.</returns>
        /// <remarks>Copied from CommunityToolkit.HighPerformance</remarks>
        public static bool TryUnbox<T>(object obj, out T value) where T : struct {
            if (obj.GetType() == typeof(T)) {
                value = U.Unbox<T>(obj);
                return true;
            }

            value = default;
            return false;
        }
    }
}