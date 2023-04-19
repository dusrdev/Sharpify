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
        public static double RollingAverage(double oldAverage, double newNumber, int sampleCount) {
            if (sampleCount < 0)
                throw new ArgumentException("Count must be greater than or equal to 0", nameof(sampleCount));

            if (sampleCount is 0)
                return newNumber;

            double denominator = 1d / sampleCount;
            return (oldAverage * (sampleCount - 1) * denominator) + (newNumber * denominator);
        }

        /// <summary>
        /// Returns the factorial result of <paramref name="n"/>
        /// </summary>
        /// <param name="n"></param>
        public static double Factorial(double n) {
            if (n <= 0) {
                throw new ArgumentException("Number must be greater than 0", nameof(n));
            }

            if (n is 1 or 2) {
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
            var denominator = two * sqrt5;
            return numerator / denominator;
        }
    }
}