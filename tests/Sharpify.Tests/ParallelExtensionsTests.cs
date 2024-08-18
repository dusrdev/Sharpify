using System.Collections;
using System.Collections.Concurrent;

namespace Sharpify.Tests;

public class ParallelExtensionsTests {
    [Fact]
    public async Task InvokeAsync_WithAsyncLocal_ReturnsValidResult() {
        // Arrange
        List<int> collection = new() { 1, 2, 3 };
        var results = new ConcurrentDictionary<int, int>();
        var action = new MultiplyActionAsync(results);

        // Act
        await collection.AsAsyncLocal(default(int)).InvokeAsync(action);

        // Assert
        results.Should().Equal(new Dictionary<int, int> {
			{ 1, 2 },
			{ 2, 4 },
			{ 3, 6 }
		});
    }

    [Fact]
    public async Task ForeachValueTask_GiveValidCollection_ReturnsValidResult() {
        // Arrange
        List<int> collection = new() { 1, 2, 3 };
        var results = new ConcurrentDictionary<int, int>();
        var action = new MultiplyAction(results);

        List<ArrayList> list = new();
        list.AsAsyncLocal(default(ArrayList));

        // Act
        await collection.AsAsyncLocal<List<int>, int>().ForEach(action);

        // Assert
        results.Should().Equal(new Dictionary<int, int> {
			{ 1, 2 },
			{ 2, 4 },
			{ 3, 6 }
		});
    }

    [Fact]
    public async Task WhenAllAsync_WithValidAsyncLocal_ReturnsValidResult() {
        // Arrange
        var dict = Enumerable.Range(1, 100).ToDictionary(x => x, x => x);
        var results = new ConcurrentDictionary<int, int>(Environment.ProcessorCount, dict.Count);
        var action = new MultiplyValueActionDictAsync(results);

        var (buffer, entries) = dict.RentBufferAndCopyEntries();


        // Act
        await entries.AsAsyncLocal(default(KeyValuePair<int,int>)).WhenAllAsync(action);
        buffer.ReturnBufferToSharedArrayPool();
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