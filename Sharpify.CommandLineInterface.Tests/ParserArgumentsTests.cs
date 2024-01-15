namespace Sharpify.CommandLineInterface.Tests;

public class ParserArgumentsTests {
	[Fact]
	public void Split_WhenEmpty_ReturnsEmptyList() {
		Parser.Split("").Should().BeEmpty();
	}

	[Theory]
	[InlineData("hello", new[] { "hello" })]
	[InlineData("hello world", new[] { "hello", "world" })]
	[InlineData("\"hello world\"", new[] { "hello world" })]
	[InlineData("\"hello world\" \"hello world\"", new[] { "hello world", "hello world" })]
	public void Split_WhenValid_ReturnsValid(string input, string[] expected) {
		Parser.Split(input).Should().BeEquivalentTo(expected);
	}

	[Fact]
	public void Parse_WhenEmpty_ReturnsNull() {
		Parser.ParseArguments("").Should().BeNull();
	}

	[Fact]
	public void Parse_And_Arguments_Combined_Valid() {
		const string input = "command --message \"hello world\" --code 404 --force";
		var arguments = Parser.ParseArguments(input);
		arguments.Should().NotBeNull();
		var containsCommandAtPosition0 = arguments!.TryGetValue(0, out var command);
		containsCommandAtPosition0.Should().BeTrue();
		command.Should().Be("command");
		var containsMessage = arguments.TryGetValue("message", out var message);
		containsMessage.Should().BeTrue();
		message.Should().Be("hello world");
		var containsCode = arguments.TryGetValue("code", 12, out var code);
		containsCode.Should().BeTrue();
		code.Should().Be(404);
		var containsForceSwitch = arguments.Contains("force");
		containsForceSwitch.Should().BeTrue();
	}

	[Fact]
	public void Arguments_ForwardPositional_Works() {
		const string input = "command delete --code 404 --force";
		var arguments = Parser.ParseArguments(input);
		arguments.Should().NotBeNull();
		var containsCommandAtPosition0 = arguments!.TryGetValue(0, out var command);
		containsCommandAtPosition0.Should().BeTrue();
		command.Should().Be("command");
		var containsDelete = arguments.TryGetValue(1, out var delete);
		containsDelete.Should().BeTrue();
		delete.Should().Be("delete");
		var forwarded = arguments.ForwardPositionalArguments();
		forwarded.Should().NotBeNull();
		var first = forwarded!.TryGetValue(0, out var firstArg);
		first.Should().BeTrue();
		firstArg.Should().Be("delete");
	}
}