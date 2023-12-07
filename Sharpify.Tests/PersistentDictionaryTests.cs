using Sharpify.Collections;

namespace Sharpify.Tests;

public class SpecialCollectionsTests {
    [Fact]
    public void AsSpan_GivenNonEmptyList_ReturnsCorrectSpan() {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var span = list.AsSpan();

        // Assert
        span.Length.Should().Be(list.Count);
        for (int i = 0; i < list.Count; i++) {
            span[i].Should().Be(list[i]);
        }
    }

    [Fact]
    public void LocalPersistentDictionary_ReadKey_Null_WhenDoesNotExist() {
        // Arrange
        var path = Utils.Env.PathInBaseDirectory("pdict.json");
        if (File.Exists(path)) {
            File.Delete(path);
        }
        var dict = new TestLocalPersistentDictionary(path);

        // Act
        var result = dict["test"];

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LocalPersistentDictionary_ReadKey_Valid_WhenExists() {
        // Arrange
        var path = Utils.Env.PathInBaseDirectory("pdict.json");
        if (File.Exists(path)) {
            File.Delete(path);
        }
        var dict = new TestLocalPersistentDictionary(path);

        // Act
        await dict.Upsert("one", "1");

        // Assert
        dict["one"].Should().Be("1");
    }

    [Fact]
    public async Task LocalPersistentDictionary_GetOrCreate() {
        // Arrange
        var path = Utils.Env.PathInBaseDirectory("pdict.json");
        if (File.Exists(path)) {
            File.Delete(path);
        }
        var dict = new TestLocalPersistentDictionary(path);

        // Act
        var result = await dict.GetOrCreateAsync("one", "1");
        var check = dict["one"] is "1";

        // Assert
        result.Should().Be("1");
        check.Should().BeTrue();
    }

    [Fact]
    public async Task LocalPersistentDictionary_Upsert_Concurrent_SerializesOnce() {
        // Arrange
        var path = Utils.Env.PathInBaseDirectory("pdictC.json");
        if (File.Exists(path)) {
            File.Delete(path);
        }
        var dict = new TestLocalPersistentDictionary(path);
        var reset = new ManualResetEventSlim(false);

        // Act
        var upsertTasks = new Task[5];
        for (int i = 0; i < upsertTasks.Length; i++) {
            upsertTasks[i] = Task.Run(async () => {
                reset.Wait();
                await dict.Upsert($"key{i}", $"value{i}");
            });
        }
        reset.Set();
        await Task.WhenAll(upsertTasks);

        // Assert
        dict.SerializedCount.Should().BeLessThanOrEqualTo(upsertTasks.Length);
        await Task.Delay(250); // Give it a moment to serialize
        var sdict = new LocalPersistentDictionary(path);
        sdict.Count.Should().Be(upsertTasks.Length);
    }

    private readonly struct UpsertAction : IAsyncAction<int> {
        private readonly TestLocalPersistentDictionary _dict;

        public UpsertAction(TestLocalPersistentDictionary dict) {
            _dict = dict;
        }

        public Task InvokeAsync(int item) {
            return _dict.Upsert($"key{item}", $"value{item}").AsTask();
        }
    }

    [Fact]
    public void LazyLocalPersistentDictionary_ReadKey_Null_WhenDoesNotExist() {
        // Arrange
        var path = Utils.Env.PathInBaseDirectory("lpdict.json");
        if (File.Exists(path)) {
            File.Delete(path);
        }
        var dict = new LazyLocalPersistentDictionary(path);

        // Act
        var result = dict["test"];

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LazyLocalPersistentDictionary_ReadKey_Valid_WhenExists() {
        // Arrange
        var path = Utils.Env.PathInBaseDirectory("lpdict.json");
        if (File.Exists(path)) {
            File.Delete(path);
        }
        var dict = new LazyLocalPersistentDictionary(path);

        // Act
        await dict.Upsert("one", "1");

        // Assert
        dict["one"].Should().Be("1");
    }
}

public class TestLocalPersistentDictionary : LocalPersistentDictionary {
    private volatile int _serializedCount;

    public TestLocalPersistentDictionary(string path) : base(path) {
    }

    public int SerializedCount => _serializedCount;

    public override async Task SerializeDictionaryAsync() {
        Interlocked.Increment(ref _serializedCount);
        await base.SerializeDictionaryAsync();
    }
}
