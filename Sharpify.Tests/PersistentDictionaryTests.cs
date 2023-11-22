using Sharpify.Collections;

namespace Sharpify.Tests;

public class PersistentDictionaryTests {
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

    private static readonly string DictPath = Utils.Env.PathInBaseDirectory("pdict.json");
    private readonly PersistentDictionary _dict = new LocalPersistentDictionary(DictPath);
    private readonly PersistentDictionary _lazyDict = new LocalPersistentDictionary(DictPath);

    [Fact]
    public async Task LocalPersistentDictionary_ReadKey_Null_WhenDoesNotExist() {
        // Arrange
        await _dict.ClearAsync();

        // Act
        var result = _dict["test"];

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LocalPersistentDictionary_ReadKey_Valid_WhenExists() {
        // Arrange
        await _dict.Upsert("one", "1");

        // Act
        var result = _dict["one"];

        // Assert
        result.Should().Be("1");
    }

    [Fact]
    public async Task LazyLocalPersistentDictionary_ReadKey_Null_WhenDoesNotExist() {
        // Arrange
        await _lazyDict.ClearAsync();

        // Act
        var result = _lazyDict["test"];

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LazyLocalPersistentDictionary_ReadKey_Valid_WhenExists() {
        // Arrange
        await _lazyDict.Upsert("one", "1");

        // Act
        var result = _lazyDict["one"];

        // Assert
        result.Should().Be("1");
    }
}
