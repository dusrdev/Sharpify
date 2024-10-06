using System.Collections.Concurrent;

namespace Sharpify.Tests;

public class ParallelExtensionsTests {
    [Fact]
    public async Task WhenAllAsync_WithValidAsyncLocal_ReturnsValidResult() {
        // Arrange
        var dict = Enumerable.Range(1, 100).ToDictionary(x => x, x => x);
        var results = new ConcurrentDictionary<int, int>(Environment.ProcessorCount, dict.Count);
        var action = new MultiplyActionDictAsync(results);

        var (buffer, entries) = dict.RentBufferAndCopyEntries();


        // Act
        await entries.ForAllAsync(action);
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

    public Task InvokeAsync(int value, CancellationToken token = default) {
        _results[value] = value * 2;
        return Task.CompletedTask;
    }
}

public readonly struct MultiplyActionDictAsync : IAsyncAction<KeyValuePair<int,int>> {
    private readonly ConcurrentDictionary<int, int> _results;

    public MultiplyActionDictAsync(ConcurrentDictionary<int, int> results) {
        _results = results;
    }

    public Task InvokeAsync(KeyValuePair<int,int> value, CancellationToken token = default) {
        _results[value.Key] = value.Value * 2;
        return Task.CompletedTask;
    }
}