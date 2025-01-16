namespace Sharpify.Tests;

public class ThreadSafeTests {
    [Fact]
    public void ThreadSafe_EmptyConstructor() {
        ThreadSafe<int> wrapper = new();

        Assert.Equal(0, wrapper.Value);
    }

    [Fact]
    public void ThreadSafe_ValueConstructor() {
        ThreadSafe<int> wrapper = new(42);

        int result = wrapper.Value;

        Assert.Equal(42, result);
    }

    [Fact]
    public void ThreadSafe_UpdateValue() {
        ThreadSafe<int> wrapper = new(5);
        const int newValue = 99;

        int result = wrapper.Modify(_ => newValue);

        Assert.Equal(newValue, result);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(2, 3, 5)]
    [InlineData(3, 4, 7)]
    public void ThreadSafe_ModifyValue(int original, int addition, int expected) {
        ThreadSafe<int> wrapper = new(original);

        int result = wrapper.Modify(value => value + addition);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(100,100)]
    [InlineData(200, 200)]
    [InlineData(300, 300)]
    public async Task ThreadSafe_MultiThreadedAccess(int amount, int expected) {
        ThreadSafe<int> wrapper = new(0);

        var tasks = Enumerable.Range(0, amount).AsParallel().Select(i => Task.Run(() => wrapper.Modify(value => value + 1)));
        await Task.WhenAll(tasks);

        Assert.Equal(expected, wrapper.Value);
    }

    [Fact]
    public void ThreadSafe_GetHashCode() {
        int val = 42;

        ThreadSafe<int> wrapper = new(val);

        int actual = wrapper.GetHashCode();
        int expected = val.GetHashCode();

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(-4, -4)]
    public void ThreadSafe_Equals(int actual, int expected) {
        ThreadSafe<int> wrapper = new(actual);

        Assert.True(wrapper.Equals(expected));
    }

    [Fact]
    public void ThreadSafe_Equals_Null() {
        int val = 42;

        ThreadSafe<int> wrapper = new(val);

        Assert.False(wrapper.Equals(null));
    }

    [Fact]
    public void ThreadSafe_Equals_ThreadSafe() {
        int val = 42;

        ThreadSafe<int> wrapper = new(val);

        Assert.True(wrapper.Equals(new ThreadSafe<int>(val)));
    }

    [Fact]
    public void ThreadSafe_Equals_Object() {
        int val = 42;

        ThreadSafe<int> wrapper = new(val);
        var other = (object)new ThreadSafe<int>(val);

        Assert.True(wrapper.Equals(other));
    }

    [Fact]
    public void ThreadSafe_Equals_NullObject() {
        int val = 42;

        ThreadSafe<int> wrapper = new(val);
        object? other = null;

        Assert.False(wrapper.Equals(other));
    }
}