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

    [Fact]
    public void TryUnbox_ForValidInput_ValidResult() {
        // Arrange
        var obj = (object) 5;

        // Act
        var result = Utils.Unsafe.TryUnbox<int>(obj, out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be(5);
    }
}