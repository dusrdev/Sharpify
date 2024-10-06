using Sharpify.Routines;

namespace Sharpify.Tests.Routines;

public class RoutineTests {
    [Theory]
    [InlineData(550, 5)]
    [InlineData(150, 1)]
    [InlineData(1050, 10)]
    [InlineData(50, 0)]
    public async Task Routine_GivenIncreaseValueFunction_IncreasesValue(int milliseconds, int expected) {
        // Arrange
        var count = 0;
        using var routine = new Routine(100).Add(() => Interlocked.Increment(ref count));

        // Act
        routine.Start();
        await Task.Delay(milliseconds);

        // Assert
        count.Should().Be(expected);
    }
}