using System.Collections.Concurrent;

namespace Sharpify.Tests;

public class ParallelExtensionsTests {
    [Fact]
    public async Task ForAll_WithAsyncAction() {
        // Arrange
        var dict = Enumerable.Range(1, 100).ToDictionary(x => x, x => x);
        var results = new ConcurrentDictionary<int, int>(Environment.ProcessorCount, dict.Count);
        var action = new MultiplyActionDict(results);

        // Act
        await dict.ForAll(action);
        var expected = dict.ToDictionary(x => x.Key, x => x.Value * 2);

        // Assert
        results.Should().Equal(expected);
        action.GetThreadCount().Should().Be(1);
    }

    [Fact]
    public async Task ForAll_WithLambda() {
        // Arrange
        var dict = Enumerable.Range(1, 100).ToDictionary(x => x, x => x);
        var results = new ConcurrentDictionary<int, int>(Environment.ProcessorCount, dict.Count);
        var threads = new ConcurrentStack<int>();

        // Act
        await dict.ForAll((x, token) => {
            results[x.Key] = x.Value * 2;
            threads.Push(Environment.CurrentManagedThreadId);
            return Task.CompletedTask;
        });
        var expected = dict.ToDictionary(x => x.Key, x => x.Value * 2);

        // Assert
        results.Should().Equal(expected);
        new HashSet<int>(threads).Count.Should().Be(1);
    }

    [Fact]
    public async Task ForAllAsync_WithAsyncAction() {
        // Arrange
        var dict = Enumerable.Range(1, 100).ToDictionary(x => x, x => x);
        var results = new ConcurrentDictionary<int, int>(Environment.ProcessorCount, dict.Count);
        var action = new MultiplyActionDictAsync(results);

        // Act
        await dict.ForAllAsync(action);
        var expected = dict.ToDictionary(x => x.Key, x => x.Value * 2);

        // Assert
        results.Should().Equal(expected);
        action.GetThreadCount().Should().Be(Environment.ProcessorCount);
    }

    [Fact]
    public async Task ForAllAsync_WithLambda() {
        // Arrange
        var dict = Enumerable.Range(1, 100).ToDictionary(x => x, x => x);
        var results = new ConcurrentDictionary<int, int>(Environment.ProcessorCount, dict.Count);
        var threads = new ConcurrentStack<int>();

        // Act
        await dict.ForAllAsync(async (x, token) => {
            results[x.Key] = x.Value * 2;
            threads.Push(Environment.CurrentManagedThreadId);
            await Task.Delay(50, token);
        });
        var expected = dict.ToDictionary(x => x.Key, x => x.Value * 2);

        // Assert
        results.Should().Equal(expected);
        new HashSet<int>(threads).Count.Should().Be(Environment.ProcessorCount);
    }

    [Fact]
    public async Task ForAllAsync_WithLambda_Asynchronous() {
        // Arrange
        var dict = Enumerable.Range(1, 100).ToDictionary(x => x, x => x);
        var results = new ConcurrentDictionary<int, int>(Environment.ProcessorCount, dict.Count);
        var threads = new ConcurrentStack<int>();

        // Act
        await dict.ForAllAsync(async (x, token) => {
            results[x.Key] = x.Value * 2;
            threads.Push(Environment.CurrentManagedThreadId);
            await Task.Delay(50, token);
        });
        var expected = dict.ToDictionary(x => x.Key, x => x.Value * 2);

        // Assert
        results.Should().Equal(expected);
        new HashSet<int>(threads).Count.Should().Be(Environment.ProcessorCount);
    }
}

public readonly struct MultiplyActionDict : IAsyncAction<KeyValuePair<int,int>> {
    private readonly ConcurrentDictionary<int, int> _results;
    private readonly ConcurrentStack<int> _threads;

    public MultiplyActionDict(ConcurrentDictionary<int, int> results) {
        _results = results;
        _threads = new ConcurrentStack<int>();
    }

    public Task InvokeAsync(KeyValuePair<int,int> value, CancellationToken token = default) {
        _results[value.Key] = value.Value * 2;
        _threads.Push(Environment.CurrentManagedThreadId);
        return Task.CompletedTask;
    }

    public int GetThreadCount() => new HashSet<int>(_threads).Count;
}

public readonly struct MultiplyActionDictAsync : IAsyncAction<KeyValuePair<int,int>> {
    private readonly ConcurrentDictionary<int, int> _results;
    private readonly ConcurrentStack<int> _threads;

    public MultiplyActionDictAsync(ConcurrentDictionary<int, int> results) {
        _results = results;
        _threads = new ConcurrentStack<int>();
    }

    public async Task InvokeAsync(KeyValuePair<int,int> value, CancellationToken token = default) {
        _results[value.Key] = value.Value * 2;
        _threads.Push(Environment.CurrentManagedThreadId);
        await Task.Delay(50, token);
    }

    public int GetThreadCount() => new HashSet<int>(_threads).Count;
}