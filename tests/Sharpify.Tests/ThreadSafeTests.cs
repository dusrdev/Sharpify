namespace Sharpify.Tests;

public class ThreadSafeTests {
    [Fact]
    public void ThreadSafe_EmptyConstructor() {
        ThreadSafe<int> wrapper = new();

        wrapper.Value.Should().Be(0);
    }

    [Fact]
    public void ThreadSafe_ValueConstructor() {
        ThreadSafe<int> wrapper = new(42);

        int result = wrapper.Value;

        result.Should().Be(42);
    }

    [Fact]
    public void ThreadSafe_UpdateValue() {
        ThreadSafe<int> wrapper = new(5);
        const int newValue = 99;

        int result = wrapper.Modify(_ => newValue);

        result.Should().Be(newValue);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(2, 3, 5)]
    [InlineData(3, 4, 7)]
    public void ThreadSafe_ModifyValue(int original, int addition, int expected) {
        ThreadSafe<int> wrapper = new(original);

        int result = wrapper.Modify(value => value + addition);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(100,100)]
    [InlineData(200, 200)]
    [InlineData(300, 300)]
    public async Task ThreadSafe_MultiThreadedAccess(int amount, int expected) {
        ThreadSafe<int> wrapper = new(0);

        var tasks = Enumerable.Range(0, amount).AsParallel().Select(i => Task.Run(() => wrapper.Modify(value => value + 1)));
        await Task.WhenAll(tasks);

        wrapper.Value.Should().Be(expected);
    }

    [Fact]
    public void ThreadSafe_GetHashCode() {
        int val = 42;

        ThreadSafe<int> wrapper = new(val);

        int actual = wrapper.GetHashCode();
        int expected = val.GetHashCode();

        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(-4, -4)]
    public void ThreadSafe_Equals(int actual, int expected) {
        ThreadSafe<int> wrapper = new(actual);

        wrapper.Equals(expected).Should().BeTrue();
    }

    [Fact]
    public void ThreadSafe_Equals_Null() {
        int val = 42;

        ThreadSafe<int> wrapper = new(val);

        wrapper.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void ThreadSafe_Equals_ThreadSafe() {
        int val = 42;

        ThreadSafe<int> wrapper = new(val);

        wrapper.Equals(new ThreadSafe<int>(val)).Should().BeTrue();
    }

    [Fact]
    public void ThreadSafe_Equals_Object() {
        int val = 42;

        ThreadSafe<int> wrapper = new(val);
        var other = (object)new ThreadSafe<int>(val);

        wrapper.Equals(other).Should().BeTrue();
    }

    [Fact]
    public void ThreadSafe_Equals_NullObject() {
        int val = 42;

        ThreadSafe<int> wrapper = new(val);
        object? other = null;

        wrapper.Equals(other).Should().BeFalse();
    }
}