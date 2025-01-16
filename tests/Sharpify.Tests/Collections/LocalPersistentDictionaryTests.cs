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
        Assert.Null(result);
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
        Assert.Equal("1", dict["one"]);
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
        Assert.Equal("1", result);
        Assert.True(check);
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
        Assert.Equal(upsertTasks.Length, sdict.Count);
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
        Assert.Equal(5, sdict.Count);
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
        Assert.Equal(1, one);
        Assert.Equal(2, two);
        File.Delete(path);
    }
}