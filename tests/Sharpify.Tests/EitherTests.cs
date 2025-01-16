namespace Sharpify.Tests;

public class EitherTests {
    [Fact]
    public void ImplicitOperatorFromT0_CreatesEitherWithT0Value() {
        // Arrange
        Either<int, string> either = 42;

        // Assert
        Assert.True(either.IsT0);
        Assert.Equal(42, either.AsT0);
    }

    [Fact]
    public void ImplicitOperatorFromT1_CreatesEitherWithT1Value() {
        // Arrange
        Either<int, string> either = "Hello";

        // Assert
        Assert.True(either.IsT1);
        Assert.Equal("Hello", either.AsT1);
    }

    [Fact]
    public void Switch_WhenT0Value_IsUsed_CallsT0Handler() {
        // Arrange
        Either<int, string> either = 42;
        bool t0HandlerCalled = false;
        bool t1HandlerCalled = false;

        // Act
        either.Switch(t0 => t0HandlerCalled = true, t1 => t1HandlerCalled = false);

        // Assert
        Assert.True(t0HandlerCalled);
        Assert.False(t1HandlerCalled);
    }

    [Fact]
    public void Switch_WhenT1Value_IsUsed_CallsT1Handler() {
        // Arrange
        Either<int, string> either = "Hello";
        bool t0HandlerCalled = false;
        bool t1HandlerCalled = false;

        // Act
        either.Switch(t0 => t0HandlerCalled = true, t1 => t1HandlerCalled = true);

        // Assert
        Assert.False(t0HandlerCalled);
        Assert.True(t1HandlerCalled);
    }

    [Fact]
    public void Match_WhenT0Value_IsUsed_ReturnsResultFromT0Handler() {
        // Arrange
        Either<int, string> either = 42;

        // Act
        var result = either.Match(t0 => t0 * 2, t1 => t1.Length);

        // Assert
        Assert.Equal(84, result);
    }

    [Fact]
    public void Match_WhenT1Value_IsUsed_ReturnsResultFromT1Handler() {
        // Arrange
        Either<int, string> either = "Hello";

        // Act
        var result = either.Match(t0 => t0 * 2, t1 => t1.Length);

        // Assert
        Assert.Equal(5, result);
    }
}