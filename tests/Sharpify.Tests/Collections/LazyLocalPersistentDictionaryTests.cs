using System.Text.Json;

using Sharpify.Collections;

namespace Sharpify.Tests.Collections;

public class LazyLocalPersistentDictionaryTests {
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

        var testJson = new {
            Name = "test",
            Age = 21
        };

        // Act
        await dict.UpsertAsync("one", JsonSerializer.Serialize(testJson));
        await dict.UpsertAsync("two", "2");

        // Assert
        dict["two"].Should().Be("2");
    }
}