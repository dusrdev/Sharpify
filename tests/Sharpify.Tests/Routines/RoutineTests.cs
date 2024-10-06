using Sharpify.Routines;

namespace Sharpify.Tests.Routines;

public class RoutineTests {
    [Theory]
    [InlineData(600, 5)]
    [InlineData(200, 1)]
    [InlineData(1100, 10)]
    [InlineData(100, 0)]
    public async Task Routine_GivenIncreaseValueFunction_IncreasesValue(int milliseconds, int expected) {
        // Arrange
        var count = 0;
        using var routine = new Routine(100).Add(() => Interlocked.Increment(ref count));

        // Act
        routine.Start();
        await Task.Delay(milliseconds);

        // Assert
        count.Should().BeGreaterThanOrEqualTo(expected);
    }
}