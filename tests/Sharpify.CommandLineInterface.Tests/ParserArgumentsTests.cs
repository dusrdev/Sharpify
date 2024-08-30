namespace Sharpify.CommandLineInterface.Tests;

public class ParserArgumentsTests {
	[Fact]
	public void Split_WhenEmpty_ReturnsEmptyList() {
		Parser.Split("").IsDisabled.Should().BeTrue();
	}

	[Theory]
	[InlineData("hello", new[] { "hello" })]
	[InlineData("hello world", new[] { "hello", "world" })]
	[InlineData("\"hello world\"", new[] { "hello world" })]
	[InlineData("\"hello world\" \"hello world\"", new[] { "hello world", "hello world" })]
	public void Split_WhenValid_ReturnsValid(string input, string[] expected) {
		Parser.Split(input).WrittenSpan.SequenceEqual(expected).Should().BeTrue();
	}

	[Fact]
	public void MapArguments_Valid() {
		var argss = new string[][] {
			["command", "--message", "hello world", "--code", "404", "--force"], // combined
			["command", "--m", "hello world", "--c", "404", "--force"], // named + switch
			["command", "-m", "hello world", "-c", "404", "--force"], // short + switch
			["command", "--attribute", "hidden", "--file", "file.txt"], // combined
			["command", "--a", "hidden", "--f", "file.txt"], // named
			["do-this", "--n", "name", "--f", "file1.txt file2.txt"], // named
			["test", "one", "--param", "value", "two"], // positional after named
		};
		var expecteds = new Dictionary<string, string>[] {
			Helper.GetMapped(("0", "command"), ("message", "hello world"), ("code", "404"), ("force", "")),
			Helper.GetMapped(("0", "command"), ("m", "hello world"), ("c", "404"), ("force", "")),
			Helper.GetMapped(("0", "command"), ("m", "hello world"), ("c", "404"), ("force", "")),
			Helper.GetMapped(("0", "command"), ("attribute", "hidden"), ("file", "file.txt")),
			Helper.GetMapped(("0", "command"), ("a", "hidden"), ("f", "file.txt")),
			Helper.GetMapped(("0", "do-this"), ("n", "name"), ("f", "file1.txt file2.txt")),
			Helper.GetMapped(("0", "test"), ("1", "one"), ("param", "value"), ("2", "two")),
		};
		for (var i = 0; i < argss.Length; i++) {
			var args = argss[i];
			var arguments = Parser.MapArguments(args, StringComparer.CurrentCultureIgnoreCase);
			arguments.Should().BeEquivalentTo(expecteds[i]);
		}
	}

	[Fact]
	public void Parse_WhenEmpty_ReturnsNull() {
		Parser.ParseArguments("").Should().BeNull();
	}

	[Fact]
	public void Parse_And_Arguments_Command_Name() {
		const string input = "command --message \"hello world\" --code 404 --force";
		var arguments = Parser.ParseArguments(input);
		arguments.Should().NotBeNull();
		arguments!.TryGetValue(0, out var command).Should().BeTrue();
		command.Should().Be("command");
	}

	[Fact]
	public void Parse_And_Arguments_Named_Argument() {
		const string input = "command --message \"hello world\" --code 404 --force";
		var arguments = Parser.ParseArguments(input);
		arguments.Should().NotBeNull();
		arguments!.TryGetValue("message", out var message).Should().BeTrue();
		message.Should().Be("hello world");
	}

	[Fact]
	public void Parse_And_Arguments_Named_Argument_Multiple() {
		const string input = "command --message \"hello world\" --code 404 --force";
		var arguments = Parser.ParseArguments(input);
		arguments.Should().NotBeNull();
		arguments!.TryGetValues("message", " ", out var message).Should().BeTrue();
		message.Should().BeEquivalentTo(["hello", "world"]);
	}

	[Fact]
	public void Parse_And_Arguments_Named_Argument_With_Aliases() {
		const string input = "command --message \"hello world\" --code 404 --force";
		var arguments = Parser.ParseArguments(input);
		arguments.Should().NotBeNull();
		arguments!.TryGetValue(["message", "m"], out var message).Should().BeTrue();
		message.Should().Be("hello world");
	}

	[Fact]
	public void Parse_And_Arguments_Named_Argument_With_Aliases_Inverted() {
		const string input = "command -m \"hello world\" --code 404 --force";
		var arguments = Parser.ParseArguments(input);
		arguments.Should().NotBeNull();
		arguments!.TryGetValue(["message", "m"], out var message).Should().BeTrue();
		message.Should().Be("hello world");
	}

	[Fact]
	public void Parse_And_Arguments_Named_Argument_Integer_WithDefault() {
		const string input = "command --message \"hello world\" --code 404 --force";
		var arguments = Parser.ParseArguments(input);
		arguments.Should().NotBeNull();
		arguments!.TryGetValue("code", 12, out var code).Should().BeTrue();
		code.Should().Be(404);
	}

	[Fact]
	public void Parse_And_Arguments_With_Flag() {
		const string input = "command --message \"hello world\" --code 404 --force";
		var arguments = Parser.ParseArguments(input);
		arguments.Should().NotBeNull();
		arguments!.HasFlag("force").Should().BeTrue();
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