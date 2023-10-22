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

    private static readonly string DictPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pdict.json");
    private readonly PersistentDictionary _dict = new LocalPersistentDictionary(DictPath);

    [Fact]
    public async Task LocalPersistentDictionary_ReadKey_Null_WhenDoesntExist() {
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
}
