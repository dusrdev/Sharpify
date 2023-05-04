namespace Sharpify.Tests;

public class ResultTests {
    [Fact]
    public void Result_DefaultConstructor_ThrowsException() {
        // Act
        Action act = () => new Result() {
            IsOk = true
        };

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ResultT_DefaultConstructor_ThrowsException() {
        // Act
        Action act = () => new Result<int>() {
            IsOk = true
        };

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Ok_ResultWithoutMessage_ReturnsResultWithIsOkTrueAndNoMessage() {
        // Act
        var result = Result.Ok();

        // Assert
        result.IsOk.Should().BeTrue();
        result.Message.Should().BeNull();
    }

    [Fact]
    public void Ok_ResultWithMessage_ReturnsResultWithIsOkTrueAndMessage() {
        // Act
        var result = Result.Ok("Success");

        // Assert
        result.IsOk.Should().BeTrue();
        result.Message.Should().Be("Success");
    }

    [Fact]
    public void Fail_ResultWithMessage_ReturnsResultWithIsOkFalseAndMessage() {
        // Act
        var result = Result.Fail("Failure");

        // Assert
        result.IsOk.Should().BeFalse();
        result.Message.Should().Be("Failure");
    }

    [Fact]
    public void Ok_ResultWithValue_ReturnsResultWithIsOkTrueAndValue() {
        // Act
        var result = Result.Ok(42);

        // Assert
        result.IsOk.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Ok_ResultWithValueAndMessage_ReturnsResultWithIsOkTrueAndValueAndMessage() {
        // Act
        var result = Result.Ok("Success", 42);

        // Assert
        result.IsOk.Should().BeTrue();
        result.Value.Should().Be(42);
        result.Message.Should().Be("Success");
    }

    [Fact]
    public void WithValue_ResultWithValue_ReturnsResultWithValueAndIsOkAndMessage() {
        // Arrange
        var result = Result.Ok("Success");

        // Act
        var valueResult = result.WithValue(42);

        // Assert
        valueResult.IsOk.Should().BeTrue();
        valueResult.Value.Should().Be(42);
        valueResult.Message.Should().Be("Success");
    }
}