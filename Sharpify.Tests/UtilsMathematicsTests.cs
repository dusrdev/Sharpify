using System.Diagnostics;

namespace Sharpify.Tests;

public partial class UtilsTests {
    [Theory]
    [InlineData(0, 5, 2, 2.5)]
    [InlineData(20, 10, 2, 15)]
    [InlineData(30, 30, 3, 30)]
    public void RollingAverage_WithVariousInputs_ReturnsCorrectResult(
        double val, double newVal, int count, double expectedResult) {
        // Arrange
        expectedResult = Math.Round(expectedResult, 15);

        // Act
        double result = Utils.Mathematics.RollingAverage(val, newVal, count);
        result = Math.Round(result, 15);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void RollingAverage_WithNegativeCount_ReturnsInput() {
        // Arrange
        double res = -1;
        const double val = 10;
        const double newVal = 15;
        const int count = -1;

        // Act
        try {
            res = Utils.Mathematics.RollingAverage(val, newVal, count);
        } catch (Exception) {
            // ignored -> Debug.Assert Failure
        }

        // Assert
        res.Should().Be(newVal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(-20)]
    public void Factorial_InvalidInput_ReturnsInput(double n) {
        double res = -1;
        // Act
        try {
            res = Utils.Mathematics.Factorial(n);
        } catch (Exception) {
            // ignored -> Debug.Assert Failure
        }

        // Assert
        res.Should().Be(n);
    }

    [Theory]
    [InlineData(5, 120)]
    [InlineData(8, 40320)]
    [InlineData(11, 39916800)]
    public void Factorial_ValidInput_ValidResult(double n, double expected) {
        // Act
        var result = Utils.Mathematics.Factorial(n);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(5, 5)]
    [InlineData(6, 8)]
    [InlineData(15, 610)]
    [InlineData(33, 3524578)]
    public void FibonacciApproximation_ValidInput_ValidResult(int n, double expected) {
        // Act
        var result = Utils.Mathematics.FibonacciApproximation(n);

        // Assert
        const double margin = 0.01;
        result.Should().BeApproximately(expected, margin);
    }
}