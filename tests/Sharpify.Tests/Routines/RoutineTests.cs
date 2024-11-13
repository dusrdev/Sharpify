using Sharpify.Routines;

namespace Sharpify.Tests.Routines;

public class RoutineTests {
    [Theory]
    [InlineData(5)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(0)]
    public async Task Routine_GivenIncreaseValueFunction_IncreasesValue(int expected) {
        // Arrange
        var count = 0;
        var tcs = new TaskCompletionSource();
        using var routine = new Routine(50).Add(() => {
            Interlocked.Increment(ref count);
            if (count >= expected) {
                tcs.TrySetResult();
            }
        });

        // Act
        routine.Start();
        await tcs.Task;

        // Assert
        count.Should().BeGreaterThanOrEqualTo(expected);
    }
}