using Sharpify.Routines;

namespace Sharpify.Tests.Routines;

public class AsyncRoutineTests {
    [Theory]
    [InlineData(5)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(0)]
    public async Task AsyncRoutine_GivenIncreaseValueFunction_IncreasesValue(int expected) {
        // Arrange
        var count = 0;
        var tcs = new TaskCompletionSource();
        var options = RoutineOptions.ExecuteInParallel;
        using var routine = new AsyncRoutine(TimeSpan.FromMilliseconds(50))
                        .ChangeOptions(options)
                        .Add(_ => {
                            var newCount = Interlocked.Increment(ref count);
                            if (newCount >= expected) {
                                tcs.TrySetResult();
                            }
                            return Task.CompletedTask;
                        });

        // Act
        _ = routine.Start();
        await tcs.Task;

        // Assert
        count.Should().BeGreaterThanOrEqualTo(expected);
    }
}