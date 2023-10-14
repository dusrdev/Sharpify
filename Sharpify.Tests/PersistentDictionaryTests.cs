using System.IO;

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

    private static readonly string DictPath = Path.Combine(Environment.GetFolder(SpecialFolders.Application), "pdict.json");
    private readonly PersistentDictionary _dict = new LocalPersistentDictionary(DictPath);

    [Fact]
    public void LocalPersistentDictionary_ReadKey_Null_WhenDoesntExist() {
    }
}
