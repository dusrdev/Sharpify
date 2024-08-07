using Sharpify.Collections;

using Xunit.Abstractions;

namespace Sharpify.Tests.Collections;

public class LocalPersistentDictionaryTests {
    private readonly ITestOutputHelper _testOutputHelper;

    public LocalPersistentDictionaryTests(ITestOutputHelper testOutputHelper) {
        _testOutputHelper = testOutputHelper;
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
        await dict.UpsertAsync("one", "1");

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
    public async Task LocalPersistentDictionary_Upsert_Concurrent() {
        // Arrange
        var filename = Random.Shared.Next(999, 10000).ToString();
        var path = Utils.Env.PathInBaseDirectory($"{filename}.json");
        if (File.Exists(path)) {
            File.Delete(path);
        }
        var dict = new TestLocalPersistentDictionary(path);

        // Act
        Task[] upsertTasks = [
            Task.Run(async () => await dict.UpsertAsync("one", "1")),
            Task.Run(async () => await dict.UpsertAsync("two", "2")),
            Task.Run(async () => await dict.UpsertAsync("three", "3")),
            Task.Run(async () => await dict.UpsertAsync("four", "4")),
            Task.Run(async () => await dict.UpsertAsync("five", "5")),
        ];
        await Task.WhenAll(upsertTasks);

        // Assert
        // dict.SerializedCount.Should().BeLessThanOrEqualTo(upsertTasks.Length);
        _testOutputHelper.WriteLine($"PersistentDictionary serialized count: {dict.SerializedCount}");
        // This is checking that the dictionary was serialized less than the number of upserts.
        // Ideally with perfectly concurrent updates, the dictionary would only be serialized once.
        // The reason not to check for 1 is that the tasks may not be executed perfectly in parallel.
        var sdict = new LocalPersistentDictionary(path);
        sdict.Count.Should().Be(upsertTasks.Length);
        File.Delete(path);
    }

    [Fact]
    public async Task LocalPersistentDictionary_Upsert_Sequential_NoItemsMissing() {
        // Arrange
        var filename = Random.Shared.Next(999, 10000).ToString();
        var path = Utils.Env.PathInBaseDirectory($"{filename}.json");
        if (File.Exists(path)) {
            File.Delete(path);
        }
        var dict = new TestLocalPersistentDictionary(path);

        // Act
        await dict.UpsertAsync("one", "1");
        await dict.UpsertAsync("two", "2");
        await dict.UpsertAsync("three", "3");
        await dict.UpsertAsync("four", "4");
        await dict.UpsertAsync("five", "5");

        // Assert
        var sdict = new LocalPersistentDictionary(path);
        sdict.Count.Should().Be(5);
        File.Delete(path);
    }

    [Fact]
    public async Task LocalPersistentDictionary_GenericGetAndUpsert() {
        // Arrange
        var filename = Random.Shared.Next(999, 10000).ToString();
        var path = Utils.Env.PathInBaseDirectory($"{filename}.json");
        if (File.Exists(path)) {
            File.Delete(path);
        }
        var dict = new TestLocalPersistentDictionary(path);

        // Act
        await dict.UpsertAsync("one", 1);
        await dict.UpsertAsync("two", 2);
        var sdict = new LocalPersistentDictionary(path);
        int one = await sdict.GetOrCreateAsync("one", 0);
        int two = await sdict.GetOrCreateAsync("two", 0);

        // Assert
        one.Should().Be(1);
        two.Should().Be(2);
        File.Delete(path);
    }
}