using System.Diagnostics;

namespace Sharpify;

public static partial class Utils {
    /// <summary>
    /// Provides utility methods for <see cref="Math"/>
    /// </summary>
    public static class Mathematics {
        /// <summary>
        /// Returns a rolling average
        /// </summary>
        /// <param name="oldAverage"></param>
        /// <param name="newNumber"></param>
        /// <param name="sampleCount"></param>
        /// <remarks>
        /// If the <paramref name="sampleCount"/> is less or equal to 0, the <paramref name="newNumber"/> is returned.
        /// <para>A message will be displayed during debug if that happens.</para>
        /// <para>An exception will not be thrown at runtime to increase performance.</para>
        /// </remarks>
        public static double RollingAverage(double oldAverage, double newNumber, int sampleCount) {
            Debug.Assert(sampleCount >= 0, "Count must be greater than or equal to 0");

            if (sampleCount is <= 0)
                return newNumber;

            double denominator = Math.ReciprocalEstimate(sampleCount);
            return (oldAverage * (sampleCount - 1) + newNumber) * denominator;
        }

        /// <summary>
        /// Returns the factorial result of <paramref name="n"/>
        /// </summary>
        /// <param name="n"></param>
        /// <remarks>
        /// If the <paramref name="n"/> is less or equal to 0, <paramref name="n"/> is returned.
        /// <para>A message will be displayed during debug if that happens.</para>
        /// <para>An exception will not be thrown at runtime to increase performance.</para>
        /// </remarks>
        public static double Factorial(double n) {
            Debug.Assert(n > 0, "Number must be greater than 0");

            if (n is <= 2) {
                return n;
            }

            var num = 1d;
            while (n > 1) {
                num *= n;
                n--;
            }
            return num;
        }

        /// <summary>
        /// Returns an estimate of the <paramref name="n"/>-th number in the Fibonacci sequence
        /// </summary>
        /// <param name="n"></param>
        public static double FibonacciApproximation(int n) {
            var sqrt5 = Math.Sqrt(5);
            var numerator = Math.Pow(1 + sqrt5, n) - Math.Pow(1 - sqrt5, n);
            var two = 1L << n; // bit hack to get 2^n
            var denominator = Math.ReciprocalEstimate(two * sqrt5);
            return numerator * denominator;
        }
    }
}