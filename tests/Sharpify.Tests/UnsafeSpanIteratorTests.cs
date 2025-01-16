namespace Sharpify.Tests;

public class UnsafeSpanIteratorTests {
    [Fact]
    public void UnsafeSpanIterator_UseLinq() {
        // Arrange
        Span<int> items = stackalloc int[100];
        Enumerable.Range(0, 100).ToArray().CopyTo(items);

        // Act
        var iterator = new UnsafeSpanIterator<int>(items);
        var sum = iterator.Select(x => x + 1).Sum(); // [0-99] -> [1-100]

        // Assert
        Assert.Equal(5050, sum);
    }

    [Fact]
    public async Task UnsafeSpanIterator_UseConcurrentlyInAsync() {
        // Arrange
        var arr = Enumerable.Range(1, 100).ToArray();
        int sum = 0;

        // Act
        async Task Increment(int item) {
            await Task.Delay(20);
            Interlocked.Add(ref sum, item);
        }
        var iterator = new UnsafeSpanIterator<int>(arr.AsSpan());
        var tasks = iterator.AsParallel().Select(Increment);
        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(5050, sum);
    }

    [Fact]
    public void UnsafeSpanIterator_Slice() {
        // Arrange
        Span<int> items = [1, 2, 3, 4, 5, 6];

        // Act
        var iterator = new UnsafeSpanIterator<int>(items);
        var slice = iterator.Slice(3, 2);

        // Assert
        Assert.Equal(slice, [4, 5]);
    }

    [Fact]
    public void UnsafeSpanIterator_ToEnumerable() {
        // Arrange
        Span<int> items = [1, 2, 3, 4, 5, 6];

        // Act
        var iterator = new UnsafeSpanIterator<int>(items);
        var count = iterator.ToEnumerable().Count();

        // Assert
        Assert.Equal(items.Length, count);
    }

    [Fact]
    public void UnsafeSpanIterator_SliceBeyondBounds_Throws() {
        // Arrange
        Span<int> items = [1, 2, 3, 4, 5, 6];

        // Act
        var iterator = new UnsafeSpanIterator<int>(items);
        var act = () => { var slice = iterator.Slice(4, 4); };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void UnsafeSpanIterator_GetByIndex() {
        // Arrange
        Span<int> items = [1, 2, 3, 4, 5, 6];

        // Act
        var iterator = new UnsafeSpanIterator<int>(items);

        // Assert
        Assert.Equal(4, iterator[3]);
    }

    [Fact]
    public void UnsafeSpanIterator_GetByIndexBeyondBounds_Throws() {
        // Arrange
        Span<int> items = [1, 2, 3, 4, 5, 6];

        // Act
        var iterator = new UnsafeSpanIterator<int>(items);
        var act = () => { var item = iterator[9]; };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }
}