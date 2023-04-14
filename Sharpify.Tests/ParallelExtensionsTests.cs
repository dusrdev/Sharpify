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

public readonly struct MultiplyAction : IAction<int> {
    private readonly ConcurrentDictionary<int, int> _results;

    public MultiplyAction(ConcurrentDictionary<int, int> results) {
        _results = results;
    }

    public void Invoke(int value) {
        _results[value] = value * 2;
    }
}