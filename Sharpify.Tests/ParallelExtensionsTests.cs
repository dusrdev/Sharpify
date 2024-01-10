using System.Buffers;
using System.Collections.Concurrent;

namespace Sharpify.Tests;

public class ParallelExtensionsTests {
    [Fact]
    public void Concurrent_GivenNullCollection_ThrowsArgumentNullException() {
        // Arrange
		#pragma warning disable
        ICollection<int> source = null;
		#pragma warning restore

        // Act
		#pragma warning disable
        Action act = () => source.Concurrent();
		#pragma warning restore

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'source')");
    }

    [Fact]
    public void Concurrent_DefaultConstructor_ThrowsException() {
        // Act
        Action act = () => new Concurrent<int>();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Concurrent_GivenValidCollection_WorksFine() {
        // Arrange
        ICollection<int> source = new List<int> { 1, 2, 3 };

        // Act
        _ = source.Concurrent();
    }

    [Fact]
    public async Task InvokeAsync_GivenEmptyCollection_DoesNothing() {
        // Arrange
        List<int> collection = new();
        var results = new ConcurrentDictionary<int, int>();
        var action = new MultiplyActionAsync(results);

        // Act
        await collection.Concurrent().InvokeAsync(action);

        // Assert
        results.Count.Should().Be(0);
    }

    [Fact]
    public async Task InvokeAsync_GiveValidCollection_ReturnsValidResult() {
        // Arrange
        List<int> collection = new() { 1, 2, 3 };
        var results = new ConcurrentDictionary<int, int>();
        var action = new MultiplyActionAsync(results);

        // Act
        await collection.Concurrent().InvokeAsync(action);

        // Assert
        results.Should().Equal(new Dictionary<int, int> {
			{ 1, 2 },
			{ 2, 4 },
			{ 3, 6 }
		});
    }

	[Fact]
    public void Foreach_GivenEmptyCollection_DoesNothing() {
        // Arrange
        List<int> collection = new();
        var results = new ConcurrentDictionary<int, int>();
        var action = new MultiplyAction(results);

        // Act
        collection.Concurrent().ForEach(action);

        // Assert
        results.Count.Should().Be(0);
    }

    [Fact]
    public void Foreach_GiveValidCollection_ReturnsValidResult() {
        // Arrange
        List<int> collection = new() { 1, 2, 3 };
        var results = new ConcurrentDictionary<int, int>();
        var action = new MultiplyAction(results);

        // Act
        collection.Concurrent().ForEach(action);

        // Assert
        results.Should().Equal(new Dictionary<int, int> {
			{ 1, 2 },
			{ 2, 4 },
			{ 3, 6 }
		});
    }

	[Fact]
    public async Task ForeachAsync_GivenEmptyCollection_DoesNothing() {
        // Arrange
        List<int> collection = new();
        var results = new ConcurrentDictionary<int, int>();
        var action = new MultiplyActionAsync(results);

        // Act
        await collection.Concurrent().ForEachAsync(action);

        // Assert
        results.Count.Should().Be(0);
    }

    [Fact]
    public async Task ForeachAsync_GiveValidCollection_ReturnsValidResult() {
        // Arrange
        List<int> collection = new() { 1, 2, 3 };
        var results = new ConcurrentDictionary<int, int>();
        var action = new MultiplyActionAsync(results);

        // Act
        await collection.Concurrent().ForEachAsync(action);

        // Assert
        results.Should().Equal(new Dictionary<int, int> {
			{ 1, 2 },
			{ 2, 4 },
			{ 3, 6 }
		});
    }

    [Fact]
    public async Task ForeachAsync_WithValidAsyncLocal_ReturnsValidResult() {
        // Arrange
        var dict = Enumerable.Range(1, 100).ToDictionary(x => x, x => x);
        var results = new ConcurrentDictionary<int, int>(Environment.ProcessorCount, dict.Count);
        var action = new MultiplyActionDictAsync(results);

        var array = ArrayPool<KeyValuePair<int, int>>.Shared.Rent(dict.Count);
        ((ICollection<KeyValuePair<int,int>>)dict).CopyTo(array, 0);
        IList<KeyValuePair<int,int>> segment = new ArraySegment<KeyValuePair<int, int>>(array, 0, dict.Count);


        // Act
        await segment.AsAsyncLocal().ForEachAsync(action, loadBalance: false);
        ArrayPool<KeyValuePair<int, int>>.Shared.Return(array);
        var expected = dict.ToDictionary(x => x.Key, x => x.Value * 2);

        // Assert
        results.Should().Equal(expected);
    }

    [Fact]
    public async Task WhenAllAsync_WithValidAsyncLocal_ReturnsValidResult() {
        // Arrange
        var dict = Enumerable.Range(1, 100).ToDictionary(x => x, x => x);
        var results = new ConcurrentDictionary<int, int>(Environment.ProcessorCount, dict.Count);
        var action = new MultiplyValueActionDictAsync(results);

        var array = ArrayPool<KeyValuePair<int, int>>.Shared.Rent(dict.Count);
        ((ICollection<KeyValuePair<int,int>>)dict).CopyTo(array, 0);
        IList<KeyValuePair<int,int>> segment = new ArraySegment<KeyValuePair<int, int>>(array, 0, dict.Count);


        // Act
        await segment.AsAsyncLocal().WhenAllAsync(action);
        ArrayPool<KeyValuePair<int, int>>.Shared.Return(array);
        var expected = dict.ToDictionary(x => x.Key, x => x.Value * 2);

        // Assert
        results.Should().Equal(expected);
    }
}

public readonly struct MultiplyActionAsync : IAsyncAction<int> {
    private readonly ConcurrentDictionary<int, int> _results;

    public MultiplyActionAsync(ConcurrentDictionary<int, int> results) {
        _results = results;
    }

    public Task InvokeAsync(int value) {
        _results[value] = value * 2;
        return Task.CompletedTask;
    }
}

public readonly struct MultiplyActionDictAsync : IAsyncAction<KeyValuePair<int,int>> {
    private readonly ConcurrentDictionary<int, int> _results;

    public MultiplyActionDictAsync(ConcurrentDictionary<int, int> results) {
        _results = results;
    }

    public Task InvokeAsync(KeyValuePair<int,int> value) {
        _results[value.Key] = value.Value * 2;
        return Task.CompletedTask;
    }
}

public readonly struct MultiplyValueActionDictAsync : IAsyncValueAction<KeyValuePair<int,int>> {
    private readonly ConcurrentDictionary<int, int> _results;

    public MultiplyValueActionDictAsync(ConcurrentDictionary<int, int> results) {
        _results = results;
    }

    public ValueTask InvokeAsync(KeyValuePair<int,int> value) {
        _results[value.Key] = value.Value * 2;
        return ValueTask.CompletedTask;
    }
}

public readonly struct MultiplyAction : IAction<int> {
    private readonly ConcurrentDictionary<int, int> _results;

    public MultiplyAction(ConcurrentDictionary<int, int> results) {
        _results = results;
    }

    public void Invoke(int value) {
        _results[value] = value * 2;
    }
}