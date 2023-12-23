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
    }
}