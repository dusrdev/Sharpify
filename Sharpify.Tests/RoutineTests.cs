using Sharpify.Routines;

namespace Sharpify.Tests;

public class RoutineTests {
    [Theory]
    [InlineData(550, 5)]
    [InlineData(150, 1)]
    [InlineData(1050, 10)]
    [InlineData(50, 0)]
    public async Task Routine_GivenIncreaseValueFunction_IncreasesValue(int milliseconds, int expected) {
        // Arrange
        var count = 0;
        using var routine = new Routine(100).Add(() => count++);

        // Act
        routine.Start();
        await Task.Delay(milliseconds);

        // Assert
        count.Should().Be(expected);
    }

    [Theory]
    [InlineData(550, 5)]
    [InlineData(150, 1)]
    [InlineData(1050, 10)]
    [InlineData(50, 0)]
    public async Task AsyncRoutine_GivenIncreaseValueFunction_IncreasesValue(int milliseconds, int expected) {
        // Arrange
        var count = 0;
        var options = RoutineOptions.ExecuteInParallel;
        using var routine = new AsyncRoutine(TimeSpan.FromMilliseconds(100))
                        .ChangeOptions(options)
                        .Add(x => Task.FromResult(count++));

        // Act
        _ = routine.Start();
        await Task.Delay(milliseconds);

        // Assert
        count.Should().Be(expected);
    }
}