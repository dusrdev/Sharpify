using System.Collections.ObjectModel;

namespace Sharpify.CommandLineInterface.Tests;

public class ParserArgumentsTests {
	[Fact]
	public void Split_WhenEmpty_ReturnsEmptyList() {
		Assert.Empty(Parser.Split(""));
	}

	[Theory]
	[InlineData("hello", new[] { "hello" })]
	[InlineData("hello world", new[] { "hello", "world" })]
	[InlineData("\"hello world\"", new[] { "hello world" })]
	[InlineData("\"hello world\" \"hello world\"", new[] { "hello world", "hello world" })]
	public void Split_WhenValid_ReturnsValid(string input, string[] expected) {
		Assert.Equal(expected, Parser.Split(input));
	}

	[Fact]
	public void MapArguments_Valid() {
		var args = new string[][] {
			["command", "--message", "hello world", "--code", "404", "--force"], // combined
			["command", "--m", "hello world", "--c", "404", "--force"], // named + switch
			["command", "-m", "hello world", "-c", "404", "--force"], // short + switch
			["command", "--attribute", "hidden", "--file", "file.txt"], // combined
			["command", "--a", "hidden", "--f", "file.txt"], // named
			["do-this", "--n", "name", "--f", "file1.txt file2.txt"], // named
			["test", "one", "--param", "value", "two"], // positional after named
		};
		var expected = new Dictionary<string, string>[] {
			Helper.GetMapped(("0", "command"), ("message", "hello world"), ("code", "404"), ("force", "")),
			Helper.GetMapped(("0", "command"), ("m", "hello world"), ("c", "404"), ("force", "")),
			Helper.GetMapped(("0", "command"), ("m", "hello world"), ("c", "404"), ("force", "")),
			Helper.GetMapped(("0", "command"), ("attribute", "hidden"), ("file", "file.txt")),
			Helper.GetMapped(("0", "command"), ("a", "hidden"), ("f", "file.txt")),
			Helper.GetMapped(("0", "do-this"), ("n", "name"), ("f", "file1.txt file2.txt")),
			Helper.GetMapped(("0", "test"), ("1", "one"), ("param", "value"), ("2", "two")),
		};
		for (var i = 0; i < args.Length; i++) {
			var localArgs = args[i].AsReadOnly();
			var localArguments = Parser.MapArguments(localArgs, StringComparer.CurrentCultureIgnoreCase);
			Assert.Equal(expected[i], localArguments);
		}
	}

	[Fact]
	public void Parse_WhenEmpty_ReturnsValidButEmptyArguments() {
		Assert.Equal(0, Parser.ParseArguments("").Count);
	}

	[Fact]
	public void ParseArguments_ForCollection_List() {
		List<string> args = ["command", "--message", "hello world", "--code", "404", "--force"];
		var arguments = Parser.ParseArguments(args, StringComparer.OrdinalIgnoreCase);
		Assert.NotNull(arguments);
		Assert.Equal(4, arguments.Count);
		Assert.Equal("command", arguments.GetValue(0, ""));
		Assert.Equal("hello world", arguments.GetValue("message", ""));
		Assert.Equal(404, arguments.GetValue("code", 0));
		Assert.True(arguments.HasFlag("force"));
	}

	[Fact]
	public void ParseArguments_ForCollection_Array() {
		string[] args = ["command", "--message", "hello world", "--code", "404", "--force"];
		var arguments = Parser.ParseArguments(args, StringComparer.OrdinalIgnoreCase);
		Assert.NotNull(arguments);
		Assert.Equal(4, arguments.Count);
		Assert.Equal("command", arguments.GetValue(0, ""));
		Assert.Equal("hello world", arguments.GetValue("message", ""));
		Assert.Equal(404, arguments.GetValue("code", 0));
		Assert.True(arguments.HasFlag("force"));
	}

	[Fact]
	public void ParseArguments_ForCollection_ReadOnlyCollection() {
		ReadOnlyCollection<string> roc = new(["command", "--message", "hello world", "--code", "404", "--force"]);
		var arguments = Parser.ParseArguments(roc, StringComparer.OrdinalIgnoreCase);
		Assert.NotNull(arguments);
		Assert.Equal(4, arguments.Count);
		Assert.Equal("command", arguments.GetValue(0, ""));
		Assert.Equal("hello world", arguments.GetValue("message", ""));
		Assert.Equal(404, arguments.GetValue("code", 0));
		Assert.True(arguments.HasFlag("force"));
	}

	[Fact]
	public void Parse_And_Arguments_Command_Name() {
		const string input = "command --message \"hello world\" --code 404 --force";
		var arguments = Parser.ParseArguments(input);
		Assert.NotNull(arguments);
		Assert.True(arguments!.TryGetValue(0, out var command));
		Assert.Equal("command", command);
	}

	[Fact]
	public void Parse_And_Arguments_Named_Argument() {
		const string input = "command --message \"hello world\" --code 404 --force";
		var arguments = Parser.ParseArguments(input);
		Assert.NotNull(arguments);
		Assert.True(arguments!.TryGetValue("message", out var message));
		Assert.Equal("hello world", message);
	}

	[Fact]
	public void Parse_And_Arguments_Named_Argument_Multiple() {
		const string input = "command --message \"hello world\" --code 404 --force";
		var arguments = Parser.ParseArguments(input);
		Assert.NotNull(arguments);
		Assert.True(arguments!.TryGetValues("message", " ", out var message));
		Assert.Equal(["hello", "world"], message);
	}

	[Fact]
	public void Parse_And_Arguments_Named_Argument_With_Aliases() {
		const string input = "command --message \"hello world\" --code 404 --force";
		var arguments = Parser.ParseArguments(input);
		Assert.NotNull(arguments);
		Assert.True(arguments!.TryGetValue(["message", "m"], out var message));
		Assert.Equal("hello world", message);
	}

	[Fact]
	public void Parse_And_Arguments_Named_Argument_With_Aliases_Inverted() {
		const string input = "command -m \"hello world\" --code 404 --force";
		var arguments = Parser.ParseArguments(input);
		Assert.NotNull(arguments);
		Assert.True(arguments!.TryGetValue(["message", "m"], out var message));
		Assert.Equal("hello world", message);
	}

	[Fact]
	public void Parse_And_Arguments_Named_Argument_Integer_WithDefault() {
		const string input = "command --message \"hello world\" --code 404 --force";
		var arguments = Parser.ParseArguments(input);
		Assert.NotNull(arguments);
		Assert.True(arguments!.TryGetValue("code", 12, out var code));
		Assert.Equal(404, code);
	}

	[Fact]
	public void Parse_And_Arguments_With_Flag() {
		const string input = "command --message \"hello world\" --code 404 --force";
		var arguments = Parser.ParseArguments(input);
		Assert.NotNull(arguments);
		Assert.True(arguments!.HasFlag("force"));
	}

	[Fact]
	public void Parse_And_Arguments_Positional_Negative_Numeric() {
		const string input = "command -5 -9";
		var arguments = Parser.ParseArguments(input);
		Assert.NotNull(arguments);
		Assert.True(arguments!.TryGetValue(1, 0, out int num));
		Assert.Equal(-5, num);
		Assert.True(arguments!.TryGetValue(2, 0, out int num2));
		Assert.Equal(-9, num2);
	}

	[Fact]
	public void Arguments_ForwardPositional_Works() {
		const string input = "command delete --code 404 --force";
		var arguments = Parser.ParseArguments(input);
		Assert.NotNull(arguments);
		var containsCommandAtPosition0 = arguments!.TryGetValue(0, out var command);
		Assert.True(containsCommandAtPosition0);
		Assert.Equal("command", command);
		var containsDelete = arguments.TryGetValue(1, out var delete);
		Assert.True(containsDelete);
		Assert.Equal("delete", delete);
		var forwarded = arguments.ForwardPositionalArguments();
		Assert.NotNull(forwarded);
		var first = forwarded!.TryGetValue(0, out var firstArg);
		Assert.True(first);
		Assert.Equal("delete", firstArg);
	}
}