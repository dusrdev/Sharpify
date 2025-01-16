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
        Assert.Equal(1, one);
        Assert.Equal(0, a);
    }

    [Fact]
    public void TryUnbox_ForValidInput_ValidResult() {
        // Arrange
        var obj = (object) 5;

        // Act
        var result = Utils.Unsafe.TryUnbox<int>(obj, out var value);

        // Assert
        Assert.True(result);
        Assert.Equal(5, value);
    }

    [Fact]
    public void AsMutableSpan_ForValidInput_ValidResult() {
        // Arrange
        string str = "abc";
        var span = str.AsSpan();

        // Act
        var mutableSpan = Utils.Unsafe.AsMutableSpan(span);
        mutableSpan[2] = '1';

        // Assert
        Assert.Equal("ab1", str);
    }
}