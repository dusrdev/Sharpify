
namespace Sharpify.Tests;

public class ThreadSafeTests {
    [Fact]
    public void GetValue_InitialValue_ReturnsInitialValue() {
        ThreadSafe<int> wrapper = new(42);

        int result = wrapper.Value;

        result.Should().Be(42);
    }

    [Fact]
    public void SetValue_NewValue_UpdatesValue() {
        ThreadSafe<int> wrapper = new(5);
        const int newValue = 99;

        int result = wrapper.Modify(_ => newValue);

        result.Should().Be(newValue);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(2, 3, 5)]
    [InlineData(3, 4, 7)]
    public void SetValue_NewValueWithModifier_UpdatesValue(int original, int addition, int expected) {
        ThreadSafe<int> wrapper = new(original);
        var adder = new Adder();

        int result = wrapper.Modify(adder, addition);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(2, 3, 5)]
    [InlineData(3, 4, 7)]
    public void PerformActionWithResult_Action_ReturnsActionResult(int original, int addition, int expected) {
        ThreadSafe<int> wrapper = new(original);

        int result = wrapper.Modify(value => value + addition);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(100, 32, 3200)]
    [InlineData(200, 64, 12800)]
    [InlineData(300, 128, 38400)]
    [InlineData(100, 10, 1000)]
    [InlineData(200, 20, 4000)]
    [InlineData(300, 30, 9000)]
    public async Task GetValue_MultiThreadedAccess_Delegate_ThreadSafe(int iterations, int threads, int expected) {
        ThreadSafe<int> wrapper = new(0);
        async Task Increment() {
            await Task.Run(() => {
                for (int i = 0; i < iterations; i++) {
                    wrapper.Modify(value => value + 1);
                }
            });
        }

        Task[] tasks = new Task[threads];
        for (int i = 0; i < tasks.Length; i++) {
            tasks[i] = Increment();
        }

        await Task.WhenAll(tasks);
        int result = wrapper.Value;

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(100, 32, 3200)]
    [InlineData(200, 64, 12800)]
    [InlineData(300, 128, 38400)]
    [InlineData(100, 10, 1000)]
    [InlineData(200, 20, 4000)]
    [InlineData(300, 30, 9000)]
    public async Task GetValue_MultiThreadedAccess_Modifier_ThreadSafe(int iterations, int threads, int expected) {
        var modifier = new Adder();
        ThreadSafe<int> wrapper = new(0);
        async Task Increment() {
            await Task.Run(() => {
                for (int i = 0; i < iterations; i++) {
                    wrapper.Modify(modifier, 1);
                }
            });
        }

        Task[] tasks = new Task[threads];
        for (int i = 0; i < tasks.Length; i++) {
            tasks[i] = Increment();
        }

        await Task.WhenAll(tasks);
        int result = wrapper.Value;

        result.Should().Be(expected);
    }

    private class Adder : IModifier<int> {
        public int Modify(int value, int newValue) => value + newValue;
    }
}