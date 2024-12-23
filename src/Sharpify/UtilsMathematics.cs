namespace Sharpify;

public static partial class Utils {
    /// <summary>
    /// Provides utility methods for <see cref="Math"/>
    /// </summary>
    public static class Mathematics {
        /// <summary>
        /// Returns a rolling average
        /// </summary>
        /// <param name="oldAverage">The previous average value</param>
        /// <param name="newNumber">The new statistic</param>
        /// <param name="sampleCount">The number of total samples, previous + 1</param>
        /// <remarks>
        /// If the <paramref name="sampleCount"/> is less or equal to 0, the <paramref name="newNumber"/> is returned.
        /// <para>A message will be displayed during debug if that happens.</para>
        /// <para>An exception will not be thrown at runtime to increase performance.</para>
        /// </remarks>
        public static double RollingAverage(double oldAverage, double newNumber, int sampleCount) {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sampleCount, 0);
            if (sampleCount is 1) {
                return newNumber;
            }
            double denominator = 1d / sampleCount;
            return ((oldAverage * (sampleCount - 1)) + newNumber) * denominator;
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
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(n, 0);
            if (n is <= 2) {
                return n;
            }
            var num = 1d;
            for (; n > 1; n--) {
                num *= n;
            }
            return num;
        }

        /// <summary>
        /// Returns an estimate of the <paramref name="n"/>-th number in the Fibonacci sequence
        /// </summary>
        public static double FibonacciApproximation(int n) {
            var sqrt5 = Math.Sqrt(5);
            var numerator = Math.Pow(1 + sqrt5, n) - Math.Pow(1 - sqrt5, n);
            var denominator = Math.ScaleB(sqrt5, n);
            return numerator / denominator;
        }
    }
}