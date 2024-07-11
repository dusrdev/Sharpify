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