namespace Sharpify.Tests;

public partial class UtilsTests {
    [Fact]
    public void CreateIntegerPredicate_ForCharIsDigit_Valid() {
        // Arrange
        var predicate = Utils.Unsafe.CreateIntegerPredicate<char>(char.IsDigit);

        // Act
        var one = predicate('1');
        var a = predicate('a');

        // Assert
        one.Should().Be(1);
        a.Should().Be(0);
    }
}