using Sharpify.Routines;

namespace Sharpify.Tests.Routines;

public class AsyncRoutineTests {
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
                        .Add(_ => {
                            Interlocked.Increment(ref count);
                            return Task.CompletedTask;
                        });

        // Act
        _ = routine.Start();
        await Task.Delay(milliseconds);

        // Assert
        count.Should().Be(expected);
    }
}