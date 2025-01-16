namespace Sharpify.Tests;

public class ResultTests {
    [Fact]
    public void Result_DefaultConstructor_ThrowsException() {
        // Act
        Action act = () => new Result() {
            IsOk = true
        };

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void ResultT_DefaultConstructor_ThrowsException() {
        // Act
        Action act = () => new Result<int>() {
            IsOk = true
        };

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Ok_ResultWithoutMessage_ReturnsResultWithIsOkTrueAndNoMessage() {
        // Act
        var result = Result.Ok();

        // Assert
        Assert.True(result.IsOk);
        Assert.Empty(result.Message);
    }

    [Fact]
    public void Ok_ResultWithMessage_ReturnsResultWithIsOkTrueAndMessage() {
        // Act
        var result = Result.Ok("Success");

        // Assert
        Assert.True(result.IsOk);
        Assert.Equal("Success", result.Message);
    }

    [Fact]
    public void Fail_ResultWithMessage_ReturnsResultWithIsOkFalseAndMessage() {
        // Act
        var result = Result.Fail("Failure");

        // Assert
        Assert.False(result.IsOk);
        Assert.Equal("Failure", result.Message);
    }

    [Fact]
    public void Ok_ResultWithValue_ReturnsResultWithIsOkTrueAndValue() {
        // Act
        var result = Result.Ok(42);

        // Assert
        Assert.True(result.IsOk);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Ok_ResultWithValueAndMessage_ReturnsResultWithIsOkTrueAndValueAndMessage() {
        // Act
        var result = Result.Ok("Success", 42);

        // Assert
        Assert.True(result.IsOk);
        Assert.Equal("Success", result.Message);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void WithValue_ResultWithValue_ReturnsResultWithValueAndIsOkAndMessage() {
        // Arrange
        var result = Result.Ok("Success");

        // Act
        var valueResult = result.WithValue(42);

        // Assert
        Assert.True(valueResult.IsOk);
        Assert.Equal("Success", valueResult.Message);
        Assert.Equal(42, valueResult.Value);
    }
}