namespace Sharpify.Tests;

public class EitherTests {
    [Fact]
    public void ImplicitOperatorFromT0_CreatesEitherWithT0Value() {
        // Arrange
        Either<int, string> either = 42;

        // Assert
        either.IsT0.Should().BeTrue();
        either.AsT0.Should().Be(42);
    }

    [Fact]
    public void ImplicitOperatorFromT1_CreatesEitherWithT1Value() {
        // Arrange
        Either<int, string> either = "Hello";

        // Assert
        either.IsT1.Should().BeTrue();
        either.AsT1.Should().Be("Hello");
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
        t0HandlerCalled.Should().BeTrue();
        t1HandlerCalled.Should().BeFalse();
    }

    [Fact]
    public void Switch_WhenT1Value_IsUsed_CallsT1Handler() {
        // Arrange
        Either<int, string> either = "Hello";
        bool t0HandlerCalled = false;
        bool t1HandlerCalled = false;

        // Act
        either.Switch(t0 => t0HandlerCalled = false, t1 => t1HandlerCalled = true);

        // Assert
        t0HandlerCalled.Should().BeFalse();
        t1HandlerCalled.Should().BeTrue();
    }

    [Fact]
    public void Match_WhenT0Value_IsUsed_ReturnsResultFromT0Handler() {
        // Arrange
        Either<int, string> either = 42;

        // Act
        var result = either.Match(t0 => t0 * 2, t1 => t1.Length);

        // Assert
        result.Should().Be(84);
    }

    [Fact]
    public void Match_WhenT1Value_IsUsed_ReturnsResultFromT1Handler() {
        // Arrange
        Either<int, string> either = "Hello";

        // Act
        var result = either.Match(t0 => t0 * 2, t1 => t1.Length);

        // Assert
        result.Should().Be(5);
    }
}