using Sharpify.Routines;

namespace Sharpify.Tests.Routines;

public class AsyncRoutineTests {
    [Theory]
    [InlineData(600, 5)]
    [InlineData(200, 1)]
    [InlineData(1100, 10)]
    [InlineData(100, 0)]
    public async Task AsyncRoutine_GivenIncreaseValueFunction_IncreasesValue(int milliseconds, int expected) {
        // Arrange
        var count = 0;
        var tcs = new TaskCompletionSource<bool>();
        var options = RoutineOptions.ExecuteInParallel;
        using var routine = new AsyncRoutine(TimeSpan.FromMilliseconds(100))
                        .ChangeOptions(options)
                        .Add(_ => {
                            var newCount = Interlocked.Increment(ref count);
                            if (newCount >= expected) {
                                tcs.TrySetResult(true);
                            }
                            return Task.CompletedTask;
                        });

        // Act
        _ = routine.Start();
        var delayTask = Task.Delay(milliseconds);
        var completedTask = await Task.WhenAny(tcs.Task, delayTask);

        // Assert
        count.Should().BeGreaterThanOrEqualTo(expected);
    }
}